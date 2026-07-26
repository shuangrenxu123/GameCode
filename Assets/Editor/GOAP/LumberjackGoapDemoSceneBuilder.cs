using GOAP.Demo;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GOAP.Editor
{
    public static class LumberjackGoapDemoSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/GOAP_Lumberjack_Demo.unity";

        [InitializeOnLoadMethod]
        private static void ScheduleCreateScene()
        {
            EditorApplication.delayCall -= EnsureSceneExists;
            EditorApplication.delayCall += EnsureSceneExists;
        }

        [MenuItem("Tools/GOAP/创建伐木工演示场景")]
        public static void CreateSceneFromMenu()
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) != null)
            {
                Debug.Log($"[GOAP Demo] 演示场景已经存在：{ScenePath}");
                return;
            }

            CreateScene();
        }

        private static void EnsureSceneExists()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                EditorApplication.delayCall += EnsureSceneExists;
                return;
            }

            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) == null)
            {
                CreateScene();
            }
        }

        private static void CreateScene()
        {
            Scene previousActiveScene = SceneManager.GetActiveScene();
            Scene demoScene = EditorSceneManager.NewScene(
                NewSceneSetup.EmptyScene,
                NewSceneMode.Additive);

            try
            {
                SceneManager.SetActiveScene(demoScene);

                GameObject environment = new("Environment");
                GameObject ground = CreatePrimitive(
                    PrimitiveType.Plane,
                    "Ground",
                    new Vector3(0f, 0f, 0f),
                    new Vector3(2f, 1f, 2f),
                    environment.transform);

                GameObject primaryTree = CreatePrimitive(
                    PrimitiveType.Cylinder,
                    "PrimaryTree_LowCost",
                    new Vector3(-6f, 1.5f, 3f),
                    new Vector3(0.8f, 1.5f, 0.8f),
                    environment.transform);
                GameObject backupTree = CreatePrimitive(
                    PrimitiveType.Cylinder,
                    "BackupTree_HighCost",
                    new Vector3(6f, 1.5f, 3f),
                    new Vector3(0.8f, 1.5f, 0.8f),
                    environment.transform);

                GameObject primaryWorkbench = CreatePrimitive(
                    PrimitiveType.Cube,
                    "PrimaryWorkbench_LowCost",
                    new Vector3(-5f, 0.75f, -4f),
                    new Vector3(2f, 1.5f, 1.5f),
                    environment.transform);
                GameObject backupWorkbench = CreatePrimitive(
                    PrimitiveType.Cube,
                    "BackupWorkbench_HighCost",
                    new Vector3(5f, 0.75f, -4f),
                    new Vector3(2f, 1.5f, 1.5f),
                    environment.transform);

                GameObject lumberjack = CreatePrimitive(
                    PrimitiveType.Capsule,
                    "Lumberjack",
                    new Vector3(0f, 1f, 0f),
                    Vector3.one,
                    null);

                LumberjackGoapDemo controller =
                    lumberjack.AddComponent<LumberjackGoapDemo>();
                controller.Configure(
                    lumberjack.transform,
                    primaryTree.transform,
                    backupTree.transform,
                    primaryWorkbench.transform,
                    backupWorkbench.transform);

                CreateCamera();
                CreateLight();

                EditorSceneManager.MarkSceneDirty(demoScene);
                EditorSceneManager.SaveScene(demoScene, ScenePath);
                AssetDatabase.SaveAssets();
                Debug.Log($"[GOAP Demo] 已创建演示场景：{ScenePath}");
            }
            finally
            {
                if (previousActiveScene.IsValid() && previousActiveScene.isLoaded)
                {
                    SceneManager.SetActiveScene(previousActiveScene);
                }

                EditorSceneManager.CloseScene(demoScene, true);
            }
        }

        private static GameObject CreatePrimitive(
            PrimitiveType primitiveType,
            string objectName,
            Vector3 position,
            Vector3 scale,
            Transform parent)
        {
            GameObject gameObject = GameObject.CreatePrimitive(primitiveType);
            gameObject.name = objectName;
            gameObject.transform.position = position;
            gameObject.transform.localScale = scale;
            if (parent != null)
            {
                gameObject.transform.SetParent(parent, true);
            }

            return gameObject;
        }

        private static void CreateCamera()
        {
            GameObject cameraObject = new("Main Camera");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(0f, 14f, -16f);
            cameraObject.transform.LookAt(new Vector3(0f, 0f, 0f));

            UnityEngine.Camera camera = cameraObject.AddComponent<UnityEngine.Camera>();
            camera.clearFlags = CameraClearFlags.Skybox;
        }

        private static void CreateLight()
        {
            GameObject lightObject = new("Directional Light");
            lightObject.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.2f;
        }
    }
}
