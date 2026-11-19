using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;\nusing UnityEngine.UI;\nusing UnityEngine.EventSystems;
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

            // Mobile touch joystick (visible in Game view and on Android).
            GameObject canvasObject = new GameObject("Mobile HUD", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            GameObject eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            eventSystem.transform.SetParent(null);

            GameObject joystickBase = new GameObject("Movement Joystick", typeof(RectTransform), typeof(Image), typeof(VirtualJoystick));
            joystickBase.transform.SetParent(canvasObject.transform, false);
            RectTransform baseRect = joystickBase.GetComponent<RectTransform>();
            baseRect.anchorMin = new Vector2(0f, 0f);
            baseRect.anchorMax = new Vector2(0f, 0f);
            baseRect.pivot = new Vector2(0.5f, 0.5f);
            baseRect.anchoredPosition = new Vector2(190f, 190f);
            baseRect.sizeDelta = new Vector2(250f, 250f);
            Image baseImage = joystickBase.GetComponent<Image>();
            baseImage.color = new Color(0.08f, 0.14f, 0.2f, 0.48f);

            GameObject joystickHandle = new GameObject("Handle", typeof(RectTransform), typeof(Image));
            joystickHandle.transform.SetParent(joystickBase.transform, false);
            RectTransform handleRect = joystickHandle.GetComponent<RectTransform>();
            handleRect.anchorMin = new Vector2(0.5f, 0.5f);
            handleRect.anchorMax = new Vector2(0.5f, 0.5f);
            handleRect.pivot = new Vector2(0.5f, 0.5f);
            handleRect.sizeDelta = new Vector2(105f, 105f);
            joystickHandle.GetComponent<Image>().color = new Color(0.35f, 0.72f, 0.9f, 0.8f);
            joystickBase.GetComponent<VirtualJoystick>().Configure(baseRect, handleRect, player.GetComponent<PlayerMotor>());

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
