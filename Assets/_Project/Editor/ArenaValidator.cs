using System;
using ArenaSurvival.Player;
using ArenaSurvival.Spawning;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace ArenaSurvival.Editor
{
    public static class ArenaValidator
    {
        public static void Validate()
        {
            EditorSceneManager.OpenScene(ArenaBuilder.ScenePath);
            foreach (GameObject root in EditorSceneManager.GetActiveScene().GetRootGameObjects())
            {
                foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
                {
                    if (GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(child.gameObject) > 0)
                    {
                        throw new InvalidOperationException("Missing script: " + child.name);
                    }
                }
            }

            PlayerSpawner spawner = UnityEngine.Object.FindFirstObjectByType<PlayerSpawner>();
            var settings = new SerializedObject(spawner);
            if (settings.FindProperty("playerPrefab").objectReferenceValue == null ||
                settings.FindProperty("spawnPoint").objectReferenceValue == null)
            {
                throw new InvalidOperationException("Spawner has an empty reference");
            }

            GameObject player = AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/_Project/Prefabs/Characters/PF_FirstPersonPlayer.prefab");
            var look = new SerializedObject(player.GetComponent<PlayerLook>());
            if (look.FindProperty("cameraTransform").objectReferenceValue == null)
            {
                throw new InvalidOperationException("Player camera reference is empty");
            }

            foreach (Renderer renderer in UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
            {
                foreach (Material material in renderer.sharedMaterials)
                {
                    if (material == null || material.shader == null || material.shader.name != "Universal Render Pipeline/Lit")
                    {
                        throw new InvalidOperationException("Unexpected material: " + renderer.name);
                    }
                }
            }

            if (GraphicsSettings.defaultRenderPipeline == null)
            {
                throw new InvalidOperationException("Render pipeline is not assigned");
            }

            Debug.Log("ARENA_VALIDATED: scene, prefab references, materials and render pipeline");
        }
    }
}
