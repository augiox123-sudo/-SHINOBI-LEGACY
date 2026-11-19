using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using ShinobiLegacy.Player;
using ShinobiLegacy.CameraSystem;
using ShinobiLegacy.Items;

namespace ShinobiLegacy.Editor
{
    /// <summary>
    /// Builds a safe social hub inspired by the broad layout of a forest-hidden shinobi village.
    /// All geometry and signage are original blockout assets.
    /// </summary>
    public static class SafeVillageSceneGenerator
    {
        private static Material groundMat, wallMat, woodMat, roofMat, stoneMat, accentMat, leafMat, waterMat;

        [MenuItem("Tools/Shinobi Legacy/Create Safe Village Hub")]
        public static void CreateVillage()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            CreateMaterials();

            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Village_Ground";
            ground.transform.localScale = new Vector3(15f, 1f, 15f);
            ground.GetComponent<Renderer>().sharedMaterial = groundMat;

            // Defensive perimeter wall with a broad, guarded main gate to the south.
            CreateWallSegment("Outer_Wall_West", new Vector3(-72f, 4f, 0f), new Vector3(3f, 8f, 150f));
            CreateWallSegment("Outer_Wall_East", new Vector3(72f, 4f, 0f), new Vector3(3f, 8f, 150f));
            CreateWallSegment("Outer_Wall_North", new Vector3(0f, 4f, 72f), new Vector3(147f, 8f, 3f));
            CreateWallSegment("Outer_Wall_South_West", new Vector3(-39f, 4f, -72f), new Vector3(69f, 8f, 3f));
            CreateWallSegment("Outer_Wall_South_East", new Vector3(39f, 4f, -72f), new Vector3(69f, 8f, 3f));
            CreateGate();

            // Main road from the gate into the central market plaza.
            CreateRoad("Main_Street", new Vector3(0f, 0.025f, -34f), new Vector3(13f, 0.08f, 76f));
            CreateRoad("Market_Cross_Street", new Vector3(0f, 0.03f, 5f), new Vector3(86f, 0.08f, 12f));
            CreateRoad("North_Administrative_Road", new Vector3(0f, 0.03f, 38f), new Vector3(12f, 0.08f, 48f));

            // Central marketplace: stalls, merchant counters and recognizable shop labels.
            CreateMarketStall("Weapon & Tool Merchant", new Vector3(-23f, 0f, 9f), new Color(0.58f, 0.22f, 0.12f));
            CreateMarketStall("Armor & Outfit Merchant", new Vector3(-8f, 0f, 17f), new Color(0.18f, 0.34f, 0.48f));
            CreateMarketStall("Scroll & Technique Merchant", new Vector3(9f, 0f, 17f), new Color(0.42f, 0.28f, 0.58f));
            CreateMarketStall("General Goods Merchant", new Vector3(24f, 0f, 9f), new Color(0.22f, 0.48f, 0.3f));
            CreateMarketStall("Food & Ramen", new Vector3(23f, 0f, -10f), new Color(0.72f, 0.42f, 0.12f));
            CreateMarketStall("Rare Materials Trader", new Vector3(-23f, 0f, -10f), new Color(0.48f, 0.4f, 0.16f));

            // Village landmarks and useful services.
            CreateBuilding("Shinobi Administration", new Vector3(0f, 0f, 48f), new Vector3(20f, 10f, 15f), new Color(0.58f, 0.3f, 0.2f));
            CreateBuilding("Ninja Academy", new Vector3(-43f, 0f, 34f), new Vector3(18f, 7f, 15f), new Color(0.42f, 0.3f, 0.2f));
            CreateBuilding("Village Hospital", new Vector3(43f, 0f, 34f), new Vector3(18f, 8f, 15f), new Color(0.72f, 0.72f, 0.62f));
            CreateBuilding("Equipment Forge", new Vector3(-43f, 0f, -25f), new Vector3(18f, 7f, 15f), new Color(0.35f, 0.36f, 0.38f));
            CreateBuilding("Mission Office", new Vector3(43f, 0f, -25f), new Vector3(18f, 7f, 15f), new Color(0.48f, 0.34f, 0.22f));

            // Monument ridge behind the administration district. Abstract stone medallions only.
            GameObject ridge = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            ridge.name = "Village_Leader_Monument_Ridge";
            ridge.transform.position = new Vector3(0f, 18f, 77f);
            ridge.transform.localScale = new Vector3(62f, 35f, 18f);
            ridge.GetComponent<Renderer>().sharedMaterial = stoneMat;
            for (int i = 0; i < 3; i++)
            {
                GameObject medallion = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                medallion.name = "Abstract_Leader_Medallion_" + (i + 1);
                medallion.transform.position = new Vector3((i - 1) * 15f, 24f, 67.5f);
                medallion.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
                medallion.transform.localScale = new Vector3(8f, 0.7f, 8f);
                medallion.GetComponent<Renderer>().sharedMaterial = accentMat;
            }

            // Residential districts, training grounds and forest approaches.
            CreateBuilding("Residential House A", new Vector3(-49f, 0f, 3f), new Vector3(12f, 6f, 11f), new Color(0.56f, 0.39f, 0.27f));
            CreateBuilding("Residential House B", new Vector3(49f, 0f, 3f), new Vector3(12f, 6f, 11f), new Color(0.56f, 0.39f, 0.27f));
            CreateBuilding("Residential House C", new Vector3(-48f, 0f, -48f), new Vector3(13f, 6f, 11f), new Color(0.56f, 0.39f, 0.27f));
            CreateBuilding("Residential House D", new Vector3(48f, 0f, -48f), new Vector3(13f, 6f, 11f), new Color(0.56f, 0.39f, 0.27f));
            CreateTrainingGround();
            CreateForestLine();

            // Playable village arrival point, wallet, inventory and isometric camera.
            GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player.name = "Player_Village_Hub";
            player.transform.position = new Vector3(0f, 1f, -58f);
            player.GetComponent<Renderer>().sharedMaterial = accentMat;
            Object.DestroyImmediate(player.GetComponent<CapsuleCollider>());
            CharacterController playerController = player.AddComponent<CharacterController>();
            playerController.height = 2f;
            playerController.radius = 0.42f;
            player.AddComponent<PlayerMotor>();
            player.AddComponent<PlayerInventory>();
            player.AddComponent<PlayerWallet>();
            player.AddComponent<EquipmentManager>();
            player.AddComponent<PlayerSaveSystem>();
            player.AddComponent<VillageShopUI>();

            GameObject hud = new GameObject("Village Mobile HUD", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas canvas = hud.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = hud.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            GameObject joystick = new GameObject("Village Movement Joystick", typeof(RectTransform), typeof(Image), typeof(VirtualJoystick));
            joystick.transform.SetParent(hud.transform, false);
            RectTransform joystickRect = joystick.GetComponent<RectTransform>();
            joystickRect.anchorMin = Vector2.zero;
            joystickRect.anchorMax = Vector2.zero;
            joystickRect.pivot = new Vector2(0.5f, 0.5f);
            joystickRect.anchoredPosition = new Vector2(175f, 175f);
            joystickRect.sizeDelta = new Vector2(250f, 250f);
            joystick.GetComponent<Image>().color = new Color(0.08f, 0.14f, 0.2f, 0.48f);
            GameObject handle = new GameObject("Handle", typeof(RectTransform), typeof(Image));
            handle.transform.SetParent(joystick.transform, false);
            RectTransform handleRect = handle.GetComponent<RectTransform>();
            handleRect.anchorMin = new Vector2(0.5f, 0.5f);
            handleRect.anchorMax = new Vector2(0.5f, 0.5f);
            handleRect.pivot = new Vector2(0.5f, 0.5f);
            handleRect.sizeDelta = new Vector2(105f, 105f);
            handle.GetComponent<Image>().color = new Color(0.35f, 0.72f, 0.9f, 0.8f);
            joystick.GetComponent<VirtualJoystick>().Configure(joystickRect, handleRect, player.GetComponent<PlayerMotor>());

            GameObject cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 12f;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 250f;
            cameraObject.AddComponent<AudioListener>();
            IsometricCameraFollow cameraFollow = cameraObject.AddComponent<IsometricCameraFollow>();
            cameraFollow.Target = player.transform;
            cameraObject.transform.position = player.transform.position + new Vector3(0f, 15f, -11f);

            GameObject lightObject = new GameObject("Village Sun");
            Light sun = lightObject.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.intensity = 1.2f;
            lightObject.transform.rotation = Quaternion.Euler(48f, -32f, 0f);

            // Arrival point and navigation labels.
            CreateMarker("VILLAGE GATE", new Vector3(0f, 0.2f, -62f));
            CreateMarker("CENTRAL MARKET", new Vector3(0f, 0.2f, 0f));
            CreateMarker("ADMINISTRATION", new Vector3(0f, 0.2f, 57f));

            if (!AssetDatabase.IsValidFolder("Assets/Scenes")) AssetDatabase.CreateFolder("Assets", "Scenes");
            const string scenePath = "Assets/Scenes/SafeVillage.unity";
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, scenePath);
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene("Assets/Scenes/PrototypeArena.unity", true),
                new EditorBuildSettingsScene(scenePath, true)
            };
            AssetDatabase.SaveAssets();
            Debug.Log("SHINOBI LEGACY: Safe Village hub created with player spawn, isometric camera, wallet and vendor buy/sell interaction.");
        }

        private static void CreateMaterials()
        {
            groundMat = MakeMaterial("Village Ground", new Color(0.22f, 0.3f, 0.2f));
            wallMat = MakeMaterial("Village Wall", new Color(0.55f, 0.48f, 0.36f));
            woodMat = MakeMaterial("Warm Timber", new Color(0.3f, 0.17f, 0.1f));
            roofMat = MakeMaterial("Dark Roof", new Color(0.12f, 0.18f, 0.2f));
            stoneMat = MakeMaterial("Monument Stone", new Color(0.38f, 0.4f, 0.4f));
            accentMat = MakeMaterial("Monument Accent", new Color(0.68f, 0.55f, 0.36f));
            leafMat = MakeMaterial("Forest Canopy", new Color(0.08f, 0.25f, 0.12f));
            waterMat = MakeMaterial("Pond Water", new Color(0.12f, 0.38f, 0.48f));
        }

        private static Material MakeMaterial(string name, Color color)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            return new Material(shader) { name = name, color = color };
        }

        private static GameObject Cube(string name, Vector3 position, Vector3 scale, Material material)
        {
            GameObject item = GameObject.CreatePrimitive(PrimitiveType.Cube);
            item.name = name;
            item.transform.position = position;
            item.transform.localScale = scale;
            item.GetComponent<Renderer>().sharedMaterial = material;
            return item;
        }

        private static void CreateWallSegment(string name, Vector3 position, Vector3 scale)
        {
            Cube(name, position, scale, wallMat);
            for (float x = -scale.x * 0.45f; x <= scale.x * 0.45f; x += 12f)
            {
                if (scale.x < 5f) break;
                Cube(name + "_Crenel", position + new Vector3(x, 4.6f, 0f), new Vector3(2f, 1.2f, 3.8f), wallMat);
            }
        }

        private static void CreateGate()
        {
            Cube("Main_Gate_Left_Pillar", new Vector3(-9f, 6f, -72f), new Vector3(4f, 12f, 5f), woodMat);
            Cube("Main_Gate_Right_Pillar", new Vector3(9f, 6f, -72f), new Vector3(4f, 12f, 5f), woodMat);
            Cube("Main_Gate_Lintel", new Vector3(0f, 12f, -72f), new Vector3(23f, 3f, 6f), roofMat);
            Cube("Main_Gate_Door_Left", new Vector3(-4.2f, 4f, -71.7f), new Vector3(7.5f, 8f, 1f), woodMat);
            Cube("Main_Gate_Door_Right", new Vector3(4.2f, 4f, -71.7f), new Vector3(7.5f, 8f, 1f), woodMat);
            CreateLabel("VILLAGE ENTRANCE", new Vector3(0f, 14.2f, -68.5f), 2.2f);
        }

        private static void CreateRoad(string name, Vector3 position, Vector3 scale)
        {
            GameObject road = Cube(name, position, scale, wallMat);
            road.transform.position = position;
        }

        private static void CreateBuilding(string name, Vector3 position, Vector3 size, Color wallColor)
        {
            Material facade = MakeMaterial(name + " Facade", wallColor);
            Cube(name + "_Structure", position + Vector3.up * (size.y * 0.5f), size, facade);
            Cube(name + "_Roof_Main", position + Vector3.up * (size.y + 0.8f), new Vector3(size.x + 3f, 1.6f, size.z + 3f), roofMat);
            Cube(name + "_Roof_Ridge", position + Vector3.up * (size.y + 1.8f), new Vector3(size.x * 0.65f, 0.8f, 2f), accentMat);
            Cube(name + "_Door", position + new Vector3(0f, 1.6f, -size.z * 0.5f - 0.1f), new Vector3(2.4f, 3.2f, 0.3f), woodMat);
            CreateLabel(name.ToUpper(), position + new Vector3(0f, size.y + 3.4f, -size.z * 0.5f - 1f), 1.5f);
        }

        private static void CreateMarketStall(string shopName, Vector3 position, Color canopyColor)
        {
            Material canopy = MakeMaterial(shopName + " Canopy", canopyColor);
            Cube(shopName + "_Counter", position + new Vector3(0f, 1f, 0f), new Vector3(7f, 2f, 3f), woodMat);
            Cube(shopName + "_Canopy", position + new Vector3(0f, 4f, 0f), new Vector3(8f, 0.7f, 4f), canopy);
            for (int x = -1; x <= 1; x += 2)
                for (int z = -1; z <= 1; z += 2)
                    Cube(shopName + "_Post", position + new Vector3(x * 3.5f, 2.1f, z * 1.7f), new Vector3(0.35f, 4f, 0.35f), woodMat);
            GameObject vendor = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            vendor.name = "Vendor_NPC_" + shopName.Replace(" ", "_").Replace("&", "and");
            vendor.transform.position = position + new Vector3(0f, 2f, 1.8f);
            vendor.transform.localScale = new Vector3(0.75f, 1f, 0.75f);
            vendor.GetComponent<Renderer>().sharedMaterial = accentMat;
            SphereCollider interaction = vendor.AddComponent<SphereCollider>();
            interaction.isTrigger = true;
            interaction.radius = 4.5f;
            VillageVendor shop = vendor.AddComponent<VillageVendor>();
            shop.shopName = shopName;
            shop.category = shopName;
            CreateLabel(shopName.ToUpper(), position + new Vector3(0f, 5.4f, -2.8f), 1.35f);
        }

        private static void CreateTrainingGround()
        {
            Cube("Training_Ground", new Vector3(0f, 0.04f, -48f), new Vector3(42f, 0.08f, 25f), groundMat);
            for (int i = 0; i < 5; i++)
            {
                GameObject post = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                post.name = "Training_Post_" + (i + 1);
                post.transform.position = new Vector3(-16f + i * 8f, 1.8f, -48f);
                post.transform.localScale = new Vector3(0.8f, 1.8f, 0.8f);
                post.GetComponent<Renderer>().sharedMaterial = woodMat;
            }
            CreateLabel("TRAINING GROUNDS", new Vector3(0f, 0.2f, -33f), 1.8f);
        }

        private static void CreateForestLine()
        {
            Random.InitState(7419);
            for (int i = 0; i < 46; i++)
            {
                float x = Random.Range(-92f, 92f);
                float z = Random.Range(-96f, 96f);
                if (Mathf.Abs(x) < 76f && Mathf.Abs(z) < 76f) continue;
                GameObject trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                trunk.name = "Forest_Tree_Trunk";
                trunk.transform.position = new Vector3(x, 3f, z);
                trunk.transform.localScale = new Vector3(1.2f, 3f, 1.2f);
                trunk.GetComponent<Renderer>().sharedMaterial = woodMat;
                GameObject crown = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                crown.name = "Forest_Tree_Canopy";
                crown.transform.position = new Vector3(x, 8f, z);
                crown.transform.localScale = new Vector3(8f, 8f, 8f);
                crown.GetComponent<Renderer>().sharedMaterial = leafMat;
            }
        }

        private static void CreateMarker(string text, Vector3 position)
        {
            GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            marker.name = "Landmark_" + text.Replace(" ", "_");
            marker.transform.position = position;
            marker.transform.localScale = new Vector3(2.2f, 0.2f, 2.2f);
            marker.GetComponent<Renderer>().sharedMaterial = waterMat;
            CreateLabel(text, position + Vector3.up * 1.5f, 1.7f);
        }

        private static void CreateLabel(string text, Vector3 position, float size)
        {
            GameObject label = new GameObject("Sign_" + text.Replace(" ", "_"));
            label.transform.position = position;
            label.transform.rotation = Quaternion.Euler(55f, 0f, 0f);
            TextMesh mesh = label.AddComponent<TextMesh>();
            mesh.text = text;
            mesh.characterSize = size;
            mesh.fontSize = 48;
            mesh.anchor = TextAnchor.MiddleCenter;
            mesh.alignment = TextAlignment.Center;
            mesh.color = new Color(1f, 0.9f, 0.68f);
        }
    }
}
