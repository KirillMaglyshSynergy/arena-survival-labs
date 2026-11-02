using System;
using System.IO;
using ArenaSurvival.Player;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEditorInternal;

namespace ArenaSurvival.Editor
{
    /// <summary>Сохраняет реальные снимки редактора для отчета.</summary>
    [InitializeOnLoad]
    public static class ArenaEvidence
    {
        private const string StateKey = "ArenaEvidence.State";
        private static double nextAction;
        private static string Output => Path.GetFullPath(Path.Combine(Application.dataPath, "../../screenshots"));

        static ArenaEvidence()
        {
            nextAction = EditorApplication.timeSinceStartup + 5;
            EditorApplication.update += Update;
        }

        public static void Capture()
        {
            Directory.CreateDirectory(Output);
            EditorSceneManager.OpenScene(ArenaBuilder.ScenePath);
            Type hierarchyType = typeof(EditorWindow).Assembly.GetType("UnityEditor.SceneHierarchyWindow");
            EditorWindow hierarchy = EditorWindow.GetWindow(hierarchyType);
            var expand = hierarchyType.GetMethod("SetExpandedRecursive",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
            foreach (GameObject root in EditorSceneManager.GetActiveScene().GetRootGameObjects())
            {
                expand?.Invoke(hierarchy, new object[] { root.GetInstanceID(), true });
            }
            SessionState.SetInt(StateKey, 1);
            EditorApplication.isPlaying = true;
        }

        private static void Update()
        {
            int state = SessionState.GetInt(StateKey, 0);
            if (state == 0 || EditorApplication.timeSinceStartup < nextAction)
            {
                return;
            }

            try
            {
                if (state == 1 && EditorApplication.isPlaying)
                {
                    GameObject player = UnityEngine.Object.FindFirstObjectByType<PlayerMovement>().gameObject;
                    Selection.activeGameObject = player;
                    Type gameViewType = typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView");
                    EditorWindow.GetWindow(gameViewType).Focus();
                    SessionState.SetInt(StateKey, 2);
                    nextAction = EditorApplication.timeSinceStartup + 4;
                }
                else if (state == 2)
                {
                    ScreenCapture.CaptureScreenshot(Path.Combine(Output, "01_game.png"));
                    SessionState.SetInt(StateKey, 6);
                    nextAction = EditorApplication.timeSinceStartup + 3;
                }
                else if (state == 6)
                {
                    CaptureEditor("02_play_editor.png");
                    EditorApplication.isPlaying = false;
                    SessionState.SetInt(StateKey, 3);
                    nextAction = EditorApplication.timeSinceStartup + 5;
                }
                else if (state == 3 && !EditorApplication.isPlayingOrWillChangePlaymode)
                {
                    Selection.activeGameObject = GameObject.Find("PlayerSpawner");
                    SceneView view = SceneView.GetWindow<SceneView>();
                    view.LookAt(new Vector3(0, 0, 0), Quaternion.Euler(55, -25, 0), 16);
                    view.Focus();
                    SessionState.SetInt(StateKey, 4);
                    nextAction = EditorApplication.timeSinceStartup + 4;
                }
                else if (state == 4)
                {
                    CaptureEditor("03_scene_editor.png");
                    var stage = UnityEditor.SceneManagement.PrefabStageUtility.OpenPrefab(
                        "Assets/_Project/Prefabs/Characters/PF_FirstPersonPlayer.prefab");
                    Selection.activeGameObject = stage.prefabContentsRoot;
                    foreach (Component component in stage.prefabContentsRoot.GetComponents<Component>())
                    {
                        InternalEditorUtility.SetIsInspectorExpanded(component,
                            component is CharacterController || component is PlayerLook);
                    }
                    EditorWindow.GetWindow(typeof(EditorWindow).Assembly.GetType("UnityEditor.InspectorWindow")).Repaint();
                    SessionState.SetInt(StateKey, 5);
                    nextAction = EditorApplication.timeSinceStartup + 4;
                }
                else if (state == 5)
                {
                    CaptureEditor("04_player_inspector.png");
                    SessionState.SetInt(StateKey, 0);
                    Debug.Log("ARENA_EVIDENCE_COMPLETE");
                    EditorApplication.Exit(0);
                }
            }
            catch (Exception exception)
            {
                SessionState.SetInt(StateKey, 0);
                Debug.LogException(exception);
                EditorApplication.Exit(1);
            }
        }

        private static void CaptureEditor(string name)
        {
            string windowType = name.StartsWith("02") ? "UnityEditor.GameView"
                : name.StartsWith("03") ? "UnityEditor.SceneView" : "UnityEditor.InspectorWindow";
            CaptureWindow(EditorWindow.GetWindow(typeof(EditorWindow).Assembly.GetType(windowType)), name);
            if (name.StartsWith("03"))
            {
                CaptureWindow(EditorWindow.GetWindow(typeof(EditorWindow).Assembly.GetType(
                    "UnityEditor.SceneHierarchyWindow")), "05_hierarchy.png");
            }
        }

        private static void CaptureWindow(EditorWindow window, string name)
        {
            const System.Reflection.BindingFlags flags = System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic;
            object view = typeof(EditorWindow).GetField("m_Parent", flags).GetValue(window);
            Type viewType = typeof(EditorWindow).Assembly.GetType("UnityEditor.GUIView");
            viewType.GetMethod("RepaintImmediately", flags).Invoke(view, null);
            int width = Mathf.RoundToInt(window.position.width * EditorGUIUtility.pixelsPerPoint);
            int height = Mathf.RoundToInt(window.position.height * EditorGUIUtility.pixelsPerPoint);
            var target = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32);
            target.Create();
            viewType.GetMethod("GrabPixels", flags).Invoke(view, new object[] {
                target, new Rect(0, 0, width, height)
            });
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = target;
            var texture = new Texture2D(width, height, TextureFormat.RGB24, false);
            texture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            Color[] rows = texture.GetPixels();
            for (int y = 0; y < height / 2; y++)
            {
                int opposite = height - y - 1;
                for (int x = 0; x < width; x++)
                {
                    int first = y * width + x;
                    int second = opposite * width + x;
                    Color saved = rows[first];
                    rows[first] = rows[second];
                    rows[second] = saved;
                }
            }
            texture.SetPixels(rows);
            texture.Apply();
            File.WriteAllBytes(Path.Combine(Output, name), texture.EncodeToPNG());
            RenderTexture.active = previous;
            target.Release();
            UnityEngine.Object.DestroyImmediate(target);
            UnityEngine.Object.DestroyImmediate(texture);
        }
    }
}
