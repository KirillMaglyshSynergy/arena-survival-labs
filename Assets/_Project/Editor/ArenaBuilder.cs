using System;
using ArenaSurvival.Player;
using ArenaSurvival.Spawning;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Object = UnityEngine.Object;

namespace ArenaSurvival.Editor
{
    /// <summary>Создает учебную сцену средствами Editor API.</summary>
    public static class ArenaBuilder
    {
        private const string Root = "Assets/_Project/";
        public const string ScenePath = Root + "Scenes/Arena_01.unity";

        [MenuItem("Arena Survival/Create laboratory scene")]
        public static void CreateScene()
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) != null)
            {
                throw new InvalidOperationException(
                    "Arena_01 already exists. The builder will not overwrite an edited scene.");
            }

            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var environment = Group("Environment");
            var gameplay = Group("Gameplay");
            var lighting = Group("Lighting");
            var walls = Group("Walls", environment);
            var obstacles = Group("Obstacles", environment);
            var staticObstacles = Group("StaticObstacles", obstacles);
            var physicsObstacles = Group("PhysicsObstacles", obstacles);

            Material floorMaterial = Material("M_Floor", new Color(0.24f, 0.30f, 0.34f));
            Material wallMaterial = Material("M_Wall", new Color(0.55f, 0.65f, 0.69f));
            Material obstacleMaterial = Material("M_Obstacle", new Color(0.85f, 0.43f, 0.17f));
            Material trimMaterial = Material("M_Trim", new Color(0.15f, 0.20f, 0.23f));

            Cube("Floor", environment, new Vector3(0, -0.25f, 0),
                new Vector3(20, 0.5f, 20), floorMaterial);

            GameObject wall = Cube("PF_Wall", null, Vector3.zero,
                new Vector3(20, 3, 0.5f), wallMaterial);
            GameObject wallPrefab = PrefabUtility.SaveAsPrefabAsset(
                wall, Root + "Prefabs/Environment/PF_Wall.prefab");
            Object.DestroyImmediate(wall);
            Place(wallPrefab, "Wall_North", walls, new Vector3(0, 1.5f, 10));
            Place(wallPrefab, "Wall_South", walls, new Vector3(0, 1.5f, -10));
            Place(wallPrefab, "Wall_East", walls, new Vector3(10, 1.5f, 0), 90);
            Place(wallPrefab, "Wall_West", walls, new Vector3(-10, 1.5f, 0), 90);

            GameObject obstacle = new GameObject("PF_Obstacle");
            BoxCollider box = obstacle.AddComponent<BoxCollider>();
            box.size = new Vector3(1.2f, 1.2f, 1.2f);
            GameObject obstacleBase = Cube("Base", obstacle.transform, Vector3.zero,
                new Vector3(1.2f, 1.2f, 1.2f), obstacleMaterial);
            Object.DestroyImmediate(obstacleBase.GetComponent<Collider>());
            GameObject top = Cube("Top", obstacle.transform, new Vector3(0, 0.45f, 0),
                new Vector3(1.22f, 0.12f, 1.22f), trimMaterial);
            Object.DestroyImmediate(top.GetComponent<Collider>());
            GameObject obstaclePrefab = PrefabUtility.SaveAsPrefabAsset(
                obstacle, Root + "Prefabs/Environment/PF_Obstacle.prefab");
            Object.DestroyImmediate(obstacle);

            Place(obstaclePrefab, "Obstacle_Static_01", staticObstacles, new Vector3(-4, 0.6f, 4));
            Place(obstaclePrefab, "Obstacle_Static_02", staticObstacles, new Vector3(4, 0.6f, 4), 20);
            AddBody(Place(obstaclePrefab, "Crate_Light_01", physicsObstacles, new Vector3(0, 0.65f, -3)), 2);
            AddBody(Place(obstaclePrefab, "Crate_Light_02", physicsObstacles, new Vector3(-3, 0.65f, 0)), 2);
            AddBody(Place(obstaclePrefab, "Crate_Light_03", physicsObstacles, new Vector3(3, 0.65f, 0), 15), 2);
            AddBody(Place(obstaclePrefab, "Crate_Heavy", physicsObstacles, new Vector3(-6, 0.65f, -3)), 20);
            AddBody(Place(obstaclePrefab, "Crate_Kinematic", physicsObstacles, new Vector3(6, 0.6f, -3)), 2, true);

            GameObject playerPrefab = CreatePlayer();
            Transform spawn = Group("PlayerSpawnPoint", gameplay);
            spawn.localPosition = new Vector3(0, 0.15f, -7);
            PlayerSpawner spawner = Group("PlayerSpawner", gameplay).gameObject.AddComponent<PlayerSpawner>();
            SetReference(spawner, "playerPrefab", playerPrefab);
            SetReference(spawner, "spawnPoint", spawn);

            Light sun = Group("Directional Light", lighting).gameObject.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.intensity = 1.6f;
            sun.shadows = LightShadows.Soft;
            sun.transform.rotation = Quaternion.Euler(50, -30, 0);
            RenderSettings.sun = sun;
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.52f, 0.58f, 0.64f);

            Volume volume = Group("Global Volume", lighting).gameObject.AddComponent<Volume>();
            volume.isGlobal = true;
            VolumeProfile profile = ScriptableObject.CreateInstance<VolumeProfile>();
            profile.name = "ArenaVolume";
            AssetDatabase.CreateAsset(profile, Root + "Settings/ArenaVolume.asset");
            Vignette vignette = profile.Add<Vignette>(true);
            vignette.intensity.Override(0.12f);
            AssetDatabase.AddObjectToAsset(vignette, profile);
            volume.sharedProfile = profile;
            EditorUtility.SetDirty(profile);

            var pipeline = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(
                Root + "Settings/PC_RPAsset.asset");
            GraphicsSettings.defaultRenderPipeline = pipeline;
            QualitySettings.renderPipeline = pipeline;
            PlayerSettings.companyName = "Coursework";
            PlayerSettings.productName = "Arena Survival";
            PlayerSettings.defaultScreenWidth = 1440;
            PlayerSettings.defaultScreenHeight = 900;
            PlayerSettings.fullScreenMode = FullScreenMode.Windowed;

            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            AssetDatabase.SaveAssets();
            Debug.Log("ARENA_CREATED: 4 walls, 7 obstacles, player prefab and spawner");
        }

        private static GameObject CreatePlayer()
        {
            var player = new GameObject("PF_FirstPersonPlayer");
            CharacterController controller = player.AddComponent<CharacterController>();
            controller.center = new Vector3(0, 0.9f, 0);
            controller.radius = 0.35f;
            controller.height = 1.8f;
            controller.slopeLimit = 45;
            controller.stepOffset = 0.3f;
            controller.skinWidth = 0.08f;
            controller.minMoveDistance = 0f;

            player.AddComponent<PlayerInputReader>();
            player.AddComponent<PlayerMovement>();
            PlayerLook look = player.AddComponent<PlayerLook>();
            ObstaclePusher pusher = player.AddComponent<ObstaclePusher>();
            var pushSettings = new SerializedObject(pusher);
            pushSettings.FindProperty("pushImpulsePerSpeedUnit").floatValue = 0.6f;
            pushSettings.ApplyModifiedPropertiesWithoutUndo();
            player.AddComponent<PlayerFallRespawner>();

            Transform cameraTransform = Group("PlayerCamera", player.transform);
            cameraTransform.localPosition = new Vector3(0, 1.6f, 0);
            Camera camera = cameraTransform.gameObject.AddComponent<Camera>();
            camera.tag = "MainCamera";
            camera.fieldOfView = 60;
            camera.nearClipPlane = 0.1f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.43f, 0.56f, 0.67f);
            cameraTransform.gameObject.AddComponent<AudioListener>();
            camera.GetUniversalAdditionalCameraData().renderPostProcessing = true;
            SetReference(look, "cameraTransform", cameraTransform);

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(
                player, Root + "Prefabs/Characters/PF_FirstPersonPlayer.prefab");
            Object.DestroyImmediate(player);
            return prefab;
        }

        private static Transform Group(string name, Transform parent = null)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            return obj.transform;
        }

        private static Material Material(string name, Color color)
        {
            var material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            material.name = name;
            material.SetColor("_BaseColor", color);
            material.SetFloat("_Smoothness", 0.25f);
            AssetDatabase.CreateAsset(material, Root + "Art/Materials/" + name + ".mat");
            return material;
        }

        private static GameObject Cube(string name, Transform parent, Vector3 position, Vector3 scale, Material material)
        {
            GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj.name = name;
            obj.transform.SetParent(parent, false);
            obj.transform.localPosition = position;
            obj.transform.localScale = scale;
            obj.GetComponent<Renderer>().sharedMaterial = material;
            return obj;
        }

        private static GameObject Place(GameObject prefab, string name, Transform parent, Vector3 position, float yaw = 0)
        {
            var obj = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            obj.name = name;
            obj.transform.localPosition = position;
            obj.transform.localRotation = Quaternion.Euler(0, yaw, 0);
            PrefabUtility.RecordPrefabInstancePropertyModifications(obj.transform);
            return obj;
        }

        private static void AddBody(GameObject obj, float mass, bool kinematic = false)
        {
            Rigidbody body = obj.AddComponent<Rigidbody>();
            body.mass = mass;
            body.isKinematic = kinematic;
            body.interpolation = RigidbodyInterpolation.Interpolate;
        }

        private static void SetReference(Object target, string field, Object value)
        {
            var serialized = new SerializedObject(target);
            serialized.FindProperty(field).objectReferenceValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
