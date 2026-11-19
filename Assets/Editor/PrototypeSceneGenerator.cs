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
using ShinobiLegacy.UI;
using ShinobiLegacy.Items;

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
            PlayerProgression progression = player.AddComponent<PlayerProgression>();
            PlayerInventory inventory = player.AddComponent<PlayerInventory>();
            EquipmentManager equipment = player.AddComponent<EquipmentManager>();

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
                CombatantHealth enemyHealth = enemy.AddComponent<CombatantHealth>();
                EnemyRewardDrop enemyRewards = enemy.AddComponent<EnemyRewardDrop>();
                enemyRewards.Configure(progression, 35, 0.72f, false);
                EnemyCombatAI ai = enemy.AddComponent<EnemyCombatAI>();
                ai.SetTarget(player.transform);
            }

            // First boss encounter: a slower heavy enemy with a telegraphed area slam.
            GameObject boss = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            boss.name = "Boss_StoneGuardian";
            boss.transform.position = new Vector3(0f, 1.6f, 14f);
            boss.transform.localScale = new Vector3(1.65f, 1.6f, 1.65f);
            boss.GetComponent<Renderer>().sharedMaterial = MakeMaterial("Boss Material", new Color(0.32f, 0.19f, 0.12f));
            Object.DestroyImmediate(boss.GetComponent<CapsuleCollider>());
            CharacterController bossController = boss.AddComponent<CharacterController>();
            bossController.height = 2f;
            bossController.radius = 0.55f;
            bossController.center = Vector3.zero;
            CombatantHealth bossHealth = boss.AddComponent<CombatantHealth>();
            bossHealth.SetMaxHealth(550f);
            EnemyRewardDrop bossRewards = boss.AddComponent<EnemyRewardDrop>();
            bossRewards.Configure(progression, 250, 1f, true);
            BossCombatAI bossAI = boss.AddComponent<BossCombatAI>();
            bossAI.SetTarget(player.transform);

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

            CreateActionButton(canvasObject.transform, "ATTACK", new Vector2(-180f, 125f), new Vector2(150f, 110f), () => playerCombat.BasicAttack());
            CreateActionButton(canvasObject.transform, "CHAKRA BURST", new Vector2(-350f, 245f), new Vector2(155f, 100f), () => playerCombat.CastChakraBurst());
            CreateActionButton(canvasObject.transform, "FIRE STYLE", new Vector2(-180f, 265f), new Vector2(135f, 90f), () => playerCombat.CastFireStyle());
            CreateActionButton(canvasObject.transform, "EQUIP BEST", new Vector2(-180f, 390f), new Vector2(150f, 76f), () => inventory.EquipBest(equipment));

            Image healthFill = CreateBar(canvasObject.transform, "Health Bar", new Vector2(35f, -35f),
                new Vector2(420f, 30f), new Color(0.78f, 0.12f, 0.12f, 1f));
            Image chakraFill = CreateBar(canvasObject.transform, "Chakra Bar", new Vector2(35f, -78f),
                new Vector2(420f, 22f), new Color(0.12f, 0.52f, 0.9f, 1f));
            GameObject statusObject = new GameObject("Skill Cooldown Status", typeof(RectTransform), typeof(Text));
            statusObject.transform.SetParent(canvasObject.transform, false);
            RectTransform statusRect = statusObject.GetComponent<RectTransform>();
            statusRect.anchorMin = new Vector2(0f, 1f);
            statusRect.anchorMax = new Vector2(0f, 1f);
            statusRect.pivot = new Vector2(0f, 1f);
            statusRect.anchoredPosition = new Vector2(35f, -112f);
            statusRect.sizeDelta = new Vector2(650f, 36f);
            GameObject progressionObject = new GameObject("Progression Status", typeof(RectTransform), typeof(Text));
            progressionObject.transform.SetParent(canvasObject.transform, false);
            RectTransform progressionRect = progressionObject.GetComponent<RectTransform>();
            progressionRect.anchorMin = new Vector2(0f, 1f);
            progressionRect.anchorMax = new Vector2(0f, 1f);
            progressionRect.pivot = new Vector2(0f, 1f);
            progressionRect.anchoredPosition = new Vector2(35f, -155f);
            progressionRect.sizeDelta = new Vector2(700f, 32f);
            Text progressionText = progressionObject.GetComponent<Text>();
            progressionText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            progressionText.fontSize = 20;
            progressionText.color = new Color(1f, 0.86f, 0.58f);
            progressionText.raycastTarget = false;
            Text statusText = statusObject.GetComponent<Text>();
            statusText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            statusText.fontSize = 20;
            statusText.color = Color.white;
            statusText.text = "SKILLS READY";
            statusText.raycastTarget = false;
            GameObject bossUi = new GameObject("Boss Health UI", typeof(RectTransform));
            bossUi.transform.SetParent(canvasObject.transform, false);
            RectTransform bossUiRect = bossUi.GetComponent<RectTransform>();
            bossUiRect.anchorMin = new Vector2(0.5f, 1f);
            bossUiRect.anchorMax = new Vector2(0.5f, 1f);
            bossUiRect.pivot = new Vector2(0.5f, 1f);
            bossUiRect.anchoredPosition = new Vector2(0f, -35f);
            bossUiRect.sizeDelta = new Vector2(560f, 70f);

            GameObject bossNameObject = new GameObject("Boss Name", typeof(RectTransform), typeof(Text));
            bossNameObject.transform.SetParent(bossUi.transform, false);
            RectTransform bossNameRect = bossNameObject.GetComponent<RectTransform>();
            bossNameRect.anchorMin = new Vector2(0f, 1f);
            bossNameRect.anchorMax = new Vector2(1f, 1f);
            bossNameRect.pivot = new Vector2(0.5f, 1f);
            bossNameRect.anchoredPosition = Vector2.zero;
            bossNameRect.sizeDelta = new Vector2(560f, 28f);
            Text bossName = bossNameObject.GetComponent<Text>();
            bossName.text = "STONE GUARDIAN";
            bossName.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            bossName.fontSize = 20;
            bossName.alignment = TextAnchor.MiddleCenter;
            bossName.color = new Color(1f, 0.82f, 0.55f);
            bossName.raycastTarget = false;

            GameObject bossBarBackground = new GameObject("Boss Health Background", typeof(RectTransform), typeof(Image));
            bossBarBackground.transform.SetParent(bossUi.transform, false);
            RectTransform bossBackgroundRect = bossBarBackground.GetComponent<RectTransform>();
            bossBackgroundRect.anchorMin = new Vector2(0f, 0f);
            bossBackgroundRect.anchorMax = new Vector2(1f, 0f);
            bossBackgroundRect.pivot = new Vector2(0.5f, 0f);
            bossBackgroundRect.anchoredPosition = Vector2.zero;
            bossBackgroundRect.sizeDelta = new Vector2(0f, 26f);
            bossBarBackground.GetComponent<Image>().color = new Color(0.025f, 0.02f, 0.02f, 0.9f);

            GameObject bossFillObject = new GameObject("Boss Health Fill", typeof(RectTransform), typeof(Image));
            bossFillObject.transform.SetParent(bossBarBackground.transform, false);
            RectTransform bossFillRect = bossFillObject.GetComponent<RectTransform>();
            bossFillRect.anchorMin = Vector2.zero;
            bossFillRect.anchorMax = Vector2.one;
            bossFillRect.offsetMin = new Vector2(3f, 3f);
            bossFillRect.offsetMax = new Vector2(-3f, -3f);
            Image bossFill = bossFillObject.GetComponent<Image>();
            bossFill.color = new Color(0.72f, 0.12f, 0.08f);
            bossFill.type = Image.Type.Filled;
            bossFill.fillMethod = Image.FillMethod.Horizontal;
            bossFill.fillAmount = 1f;
            BossHealthBar bossBar = canvasObject.AddComponent<BossHealthBar>();
            bossBar.Configure(bossHealth, bossFill, bossUi);

            CombatHud hud = canvasObject.AddComponent<CombatHud>();
            hud.Configure(player.GetComponent<CombatantHealth>(), playerCombat, healthFill, chakraFill, statusText);
            ProgressionHud progressionHud = canvasObject.AddComponent<ProgressionHud>();
            progressionHud.Configure(progression, inventory, progressionText, equipment);

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

        private static Image CreateBar(Transform parent, string name, Vector2 position, Vector2 size, Color fillColor)
        {
            GameObject backgroundObject = new GameObject(name + " Background", typeof(RectTransform), typeof(Image));
            backgroundObject.transform.SetParent(parent, false);
            RectTransform backgroundRect = backgroundObject.GetComponent<RectTransform>();
            backgroundRect.anchorMin = new Vector2(0f, 1f);
            backgroundRect.anchorMax = new Vector2(0f, 1f);
            backgroundRect.pivot = new Vector2(0f, 1f);
            backgroundRect.anchoredPosition = position;
            backgroundRect.sizeDelta = size;
            backgroundObject.GetComponent<Image>().color = new Color(0.025f, 0.035f, 0.05f, 0.82f);

            GameObject fillObject = new GameObject(name + " Fill", typeof(RectTransform), typeof(Image));
            fillObject.transform.SetParent(backgroundObject.transform, false);
            RectTransform fillRect = fillObject.GetComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = new Vector2(3f, 3f);
            fillRect.offsetMax = new Vector2(-3f, -3f);
            Image fill = fillObject.GetComponent<Image>();
            fill.color = fillColor;
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Horizontal;
            fill.fillOrigin = (int)Image.OriginHorizontal.Left;
            fill.fillAmount = 1f;
            return fill;
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
