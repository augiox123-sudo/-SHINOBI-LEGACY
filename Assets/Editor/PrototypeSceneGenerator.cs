using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using ShinobiLegacy.Player;
using ShinobiLegacy.CameraSystem;

namespace ShinobiLegacy.Editor
{
    public static class PrototypeSceneGenerator
    {
        [MenuItem("Tools/Shinobi Legacy/Create 3D Test Scene")]
        public static void CreateScene()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Arena_Ground";
            ground.transform.localScale = new Vector3(8f, 1f, 8f);
            ground.GetComponent<Renderer>().sharedMaterial = MakeMaterial("Arena Ground", new Color(0.16f, 0.24f, 0.22f));

            GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player.name = "Player_ShadowNinja_Blockout";
            player.transform.position = new Vector3(0f, 1f, 0f);
            player.GetComponent<Renderer>().sharedMaterial = MakeMaterial("Player", new Color(0.12f, 0.28f, 0.48f));
            Object.DestroyImmediate(player.GetComponent<CapsuleCollider>());
            CharacterController motorCollider = player.AddComponent<CharacterController>();
            motorCollider.height = 2f;
            motorCollider.radius = 0.42f;
            motorCollider.center = Vector3.zero;
            player.AddComponent<PlayerMotor>();

            GameObject cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 8.5f;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 150f;
            cameraObject.AddComponent<AudioListener>();
            IsometricCameraFollow follow = cameraObject.AddComponent<IsometricCameraFollow>();
            follow.Target = player.transform;
            cameraObject.transform.position = player.transform.position + new Vector3(0f, 11f, -8f);

            GameObject lightObject = new GameObject("Directional Light");
            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.15f;
            lightObject.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            for (int i = 0; i < 8; i++)
            {
                float angle = i * Mathf.PI * 2f / 8f;
                GameObject rock = GameObject.CreatePrimitive(PrimitiveType.Cube);
                rock.name = "Arena_Stone_" + (i + 1);
                rock.transform.position = new Vector3(Mathf.Cos(angle) * 10f, 0.5f, Mathf.Sin(angle) * 10f);
                rock.transform.localScale = new Vector3(1.4f, 1f, 1.4f);
                rock.GetComponent<Renderer>().sharedMaterial = MakeMaterial("Stone", new Color(0.28f, 0.31f, 0.30f));
            }

            if (!AssetDatabase.IsValidFolder("Assets/Scenes")) AssetDatabase.CreateFolder("Assets", "Scenes");
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, "Assets/Scenes/PrototypeArena.unity");
            AssetDatabase.SaveAssets();
            Selection.activeGameObject = player;
            Debug.Log("SHINOBI LEGACY: 3D prototype scene created.");
        }

        private static Material MakeMaterial(string materialName, Color color)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            Material material = new Material(shader) { name = materialName, color = color };
            return material;
        }
    }
}
