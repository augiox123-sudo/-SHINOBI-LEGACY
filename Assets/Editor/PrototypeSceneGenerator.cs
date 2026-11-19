using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using ShinobiLegacy.Player;
using ShinobiLegacy.CameraSystem;
using ShinobiLegacy.Combat;
using ShinobiLegacy.Enemies;

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
            player.AddComponent<CombatantHealth>();
            PlayerCombat playerCombat = player.AddComponent<PlayerCombat>();

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

            // Three simple melee enemies for the first combat test.
            for (int i = 0; i < 3; i++)
            {
                GameObject enemy = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                enemy.name = "TrainingEnemy_" + (i + 1);
                enemy.transform.position = new Vector3((i - 1) * 3.2f, 1f, 6f + (i % 2) * 1.5f);
                enemy.GetComponent<Renderer>().sharedMaterial = MakeMaterial("Enemy Material", new Color(0.55f, 0.12f, 0.12f));
                Object.DestroyImmediate(enemy.GetComponent<CapsuleCollider>());
                CharacterController enemyController = enemy.AddComponent<CharacterController>();
                enemyController.height = 2f;
                enemyController.radius = 0.42f;
                enemyController.center = Vector3.zero;
                enemy.AddComponent<CombatantHealth>();
                EnemyCombatAI ai = enemy.AddComponent<EnemyCombatAI>();
                ai.SetTarget(player.transform);
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

            CreateActionButton(canvasObject.transform, "Basic Attack", new Vector2(-190f, 150f), new Vector2(145f, 100f), () => playerCombat.BasicAttack());
            CreateActionButton(canvasObject.transform, "Chakra", new Vector2(-350f, 265f), new Vector2(120f, 85f), () => playerCombat.CastTechnique());

            if (!AssetDatabase.IsValidFolder("Assets/Scenes")) AssetDatabase.CreateFolder("Assets", "Scenes");
            EditorSceneManager.MarkSceneDirty(scene);
            const string scenePath = "Assets/Scenes/PrototypeArena.unity";
            EditorSceneManager.SaveScene(scene, scenePath);
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(scenePath, true)
            };
            AssetDatabase.SaveAssets();
            Selection.activeGameObject = player;
            Debug.Log("SHINOBI LEGACY: 3D prototype scene created.");
        }

        private static void CreateActionButton(Transform parent, string label, Vector2 position, Vector2 size, UnityEngine.Events.UnityAction action)
        {
            GameObject buttonObject = new GameObject(label + " Button", typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(parent, false);
            RectTransform rect = buttonObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(1f, 0f);
            rect.anchorMax = new Vector2(1f, 0f);
            rect.pivot = new Vector2(1f, 0f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            Image image = buttonObject.GetComponent<Image>();
            image.color = new Color(0.12f, 0.24f, 0.36f, 0.78f);
            Button button = buttonObject.GetComponent<Button>();
            button.onClick.AddListener(action);

            GameObject textObject = new GameObject("Label", typeof(RectTransform), typeof(Text));
            textObject.transform.SetParent(buttonObject.transform, false);
            RectTransform textRect = textObject.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            Text text = textObject.GetComponent<Text>();
            text.text = label;
            text.alignment = TextAnchor.MiddleCenter;
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.fontSize = 22;
            text.color = Color.white;
            text.raycastTarget = false;
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
