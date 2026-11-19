using UnityEngine;
using UnityEngine.UI;
using ShinobiLegacy.Combat;
using ShinobiLegacy.Items;
using ShinobiLegacy.Player;

namespace ShinobiLegacy.Enemies
{
    public sealed class DungeonWaveController : MonoBehaviour
    {
        private enum DungeonTheme { AshenRuins, VerdantForest, ThunderShrine, DrownedTemple }
        private enum DungeonDifficulty { Normal, Hard, Nightmare }

        [SerializeField] private int totalWaves = 3;
        [SerializeField] private int enemiesPerWave = 3;
        [SerializeField] private Transform player;
        [SerializeField] private Text statusLabel;
        [SerializeField] private int completionCoinReward = 250;

        private DungeonTheme selectedTheme;
        private DungeonDifficulty selectedDifficulty;
        private int currentWave;
        private int alive;
        private float nextWaveTime;
        private bool running;
        private bool waitingForNextWave;
        private bool completed;
        private Text selectionLabel;
        private Button difficultyButton;
        private Button[] themeButtons = new Button[4];
        private int lastLevelForSelectionRefresh = -1;

        public bool IsRunning => running;
        public int CurrentWave => currentWave;
        public int TotalWaves => totalWaves;

        private float DifficultyMultiplier
        {
            get
            {
                switch (selectedDifficulty)
                {
                    case DungeonDifficulty.Hard: return 1.65f;
                    case DungeonDifficulty.Nightmare: return 2.5f;
                    default: return 1f;
                }
            }
        }

        public void Configure(Transform playerTransform, Text label, int waves, int enemies)
        {
            player = playerTransform;
            statusLabel = label;
            totalWaves = Mathf.Max(1, waves);
            enemiesPerWave = Mathf.Max(1, enemies);
            SetStatus("DUNGEON READY");
            BuildSelectionUI();
            RefreshSelectionLabel();
        }

        public void StartDungeon()
        {
            if (running || player == null) return;
            if (!IsThemeUnlocked(selectedTheme))
            {
                SetStatus("DUNGEON LOCKED  |  REQUIRED LEVEL " + RequiredLevelForTheme(selectedTheme));
                return;
            }
            running = true;
            completed = false;
            waitingForNextWave = false;
            currentWave = 0;
            SpawnNextWave();
            RefreshSelectionLabel();
        }

        private void Update()
        {
            int level = GetPlayerLevel();
            if (level != lastLevelForSelectionRefresh)
            {
                lastLevelForSelectionRefresh = level;
                RefreshSelectionLabel();
            }

            if (!running || !waitingForNextWave || Time.time < nextWaveTime) return;
            waitingForNextWave = false;
            SpawnNextWave();
        }

        private void SpawnNextWave()
        {
            currentWave++;
            if (currentWave > totalWaves)
            {
                CompleteDungeon();
                return;
            }

            int regularCount = enemiesPerWave + currentWave - 1;
            bool bossWave = currentWave == totalWaves;
            alive = regularCount + (bossWave ? 1 : 0);
            SetStatus(ThemeName() + "  |  " + DifficultyName() + "  |  WAVE " +
                currentWave + "/" + totalWaves + (bossWave ? "  BOSS APPROACHING" : "  ENEMIES " + alive));

            for (int i = 0; i < regularCount; i++)
            {
                bool elite = selectedDifficulty != DungeonDifficulty.Normal &&
                    currentWave == totalWaves && i == regularCount - 1;
                SpawnEnemy(i, regularCount, elite);
            }

            if (bossWave) SpawnBoss(regularCount);
        }

        private void SpawnEnemy(int index, int count, bool elite)
        {
            float angle = (Mathf.PI * 2f * index) / Mathf.Max(1, count);
            Vector3 position = player.position + new Vector3(Mathf.Cos(angle) * 8f, 1f, Mathf.Sin(angle) * 8f);
            GameObject enemy = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            enemy.name = "Dungeon_" + selectedTheme + "_W" + currentWave + "_Enemy_" + (index + 1);
            enemy.transform.position = position;
            Renderer renderer = enemy.GetComponent<Renderer>();
            Material material = CreateMaterial();
            material.color = elite ? new Color(0.8f, 0.34f, 0.08f) : ThemeColor();
            renderer.material = material;
            Object.Destroy(enemy.GetComponent<CapsuleCollider>());

            CharacterController controller = enemy.AddComponent<CharacterController>();
            controller.height = 2f;
            controller.radius = 0.42f;
            controller.center = Vector3.zero;

            int level = GetPlayerLevel();
            float scale = DifficultyMultiplier;
            CombatantHealth health = enemy.AddComponent<CombatantHealth>();
            health.SetMaxHealth((85f + currentWave * 35f + level * 8f) * scale * (elite ? 1.8f : 1f));
            EnemyCombatAI ai = enemy.AddComponent<EnemyCombatAI>();
            ai.Configure((2.1f + currentWave * 0.18f) * (elite ? 1.12f : 1f),
                (8f + currentWave * 3f + level) * scale * (elite ? 1.35f : 1f),
                Mathf.Max(0.75f, 1.7f / scale), 20f + currentWave * 2f + level);
            ai.SetTarget(player);

            EnemyRewardDrop rewards = enemy.AddComponent<EnemyRewardDrop>();
            rewards.Configure(player.GetComponent<PlayerProgression>(),
                Mathf.RoundToInt((25 + currentWave * 15 + level * 3) * scale),
                elite ? 1f : 0.7f + currentWave * 0.08f, elite);
            if (elite) rewards.SetLegendaryChance(selectedDifficulty == DungeonDifficulty.Nightmare ? 0.16f : 0.10f);
            health.Died += OnEnemyDied;
        }

        private void SpawnBoss(int index)
        {
            Vector3 position = player.position + new Vector3(0f, 1.5f, 13f);
            GameObject boss = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            boss.name = "Dungeon_Boss_" + BossName().Replace(" ", "_");
            boss.transform.position = position;
            boss.transform.localScale = new Vector3(1.7f, 1.65f, 1.7f);
            Renderer renderer = boss.GetComponent<Renderer>();
            Material material = CreateMaterial();
            material.color = BossColor();
            renderer.material = material;
            Object.Destroy(boss.GetComponent<CapsuleCollider>());

            CharacterController controller = boss.AddComponent<CharacterController>();
            controller.height = 2.4f;
            controller.radius = 0.62f;
            controller.center = Vector3.zero;

            int level = GetPlayerLevel();
            float scale = DifficultyMultiplier;
            CombatantHealth health = boss.AddComponent<CombatantHealth>();
            health.SetMaxHealth((650f + currentWave * 180f + level * 45f) * scale);
            BossCombatAI bossAI = boss.AddComponent<BossCombatAI>();
            bossAI.SetTarget(player);

            EnemyRewardDrop rewards = boss.AddComponent<EnemyRewardDrop>();
            rewards.Configure(player.GetComponent<PlayerProgression>(),
                Mathf.RoundToInt((250 + currentWave * 100 + level * 20) * scale), 1f, true);
            float legendaryChance = selectedDifficulty == DungeonDifficulty.Nightmare ? 0.40f :
                selectedDifficulty == DungeonDifficulty.Hard ? 0.28f : 0.18f;
            rewards.SetLegendaryChance(legendaryChance);
            health.Died += OnEnemyDied;
            SetStatus("BOSS: " + BossName().ToUpper() + "  |  LEGENDARY CHANCE " +
                Mathf.RoundToInt(legendaryChance * 100f) + "%");
        }

        private int GetPlayerLevel()
        {
            PlayerProgression progression = player != null ? player.GetComponent<PlayerProgression>() : null;
            return progression != null ? progression.Level : 1;
        }

        private static Material CreateMaterial()
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            return new Material(shader);
        }

        private Color ThemeColor()
        {
            switch (selectedTheme)
            {
                case DungeonTheme.VerdantForest: return new Color(0.12f, 0.38f, 0.19f);
                case DungeonTheme.ThunderShrine: return new Color(0.28f, 0.22f, 0.58f);
                case DungeonTheme.DrownedTemple: return new Color(0.08f, 0.36f, 0.48f);
                default: return new Color(0.48f, 0.12f, 0.18f);
            }
        }


        private Color BossColor()
        {
            switch (selectedTheme)
            {
                case DungeonTheme.VerdantForest: return new Color(0.22f, 0.42f, 0.12f);
                case DungeonTheme.ThunderShrine: return new Color(0.34f, 0.24f, 0.72f);
                case DungeonTheme.DrownedTemple: return new Color(0.08f, 0.42f, 0.58f);
                default: return new Color(0.72f, 0.12f, 0.055f);
            }
        }

        private string ThemeName()
        {
            switch (selectedTheme)
            {
                case DungeonTheme.VerdantForest: return "VERDANT FOREST";
                case DungeonTheme.ThunderShrine: return "THUNDER SHRINE";
                case DungeonTheme.DrownedTemple: return "DROWNED TEMPLE";
                default: return "ASHEN RUINS";
            }
        }

        private string BossName()
        {
            switch (selectedTheme)
            {
                case DungeonTheme.VerdantForest: return "The Rootbound Colossus";
                case DungeonTheme.ThunderShrine: return "Raijin's Broken Avatar";
                case DungeonTheme.DrownedTemple: return "The Abyssal Guardian";
                default: return "The Ashen Warden";
            }
        }

        private string DifficultyName()
        {
            return selectedDifficulty.ToString().ToUpper();
        }

        private void OnEnemyDied()
        {
            if (!running) return;
            alive = Mathf.Max(0, alive - 1);
            SetStatus(ThemeName() + "  |  WAVE " + currentWave + "/" + totalWaves + "   ENEMIES " + alive);
            if (alive != 0) return;

            if (currentWave >= totalWaves)
            {
                CompleteDungeon();
            }
            else
            {
                waitingForNextWave = true;
                nextWaveTime = Time.time + 3f;
                SetStatus("WAVE CLEARED - NEXT WAVE IN 3s");
            }
        }

        private void CompleteDungeon()
        {
            if (completed) return;
            completed = true;
            running = false;
            waitingForNextWave = false;
            int coinReward = Mathf.RoundToInt((completionCoinReward + currentWave * 75) * DifficultyMultiplier);
            PlayerWallet wallet = player != null ? player.GetComponent<PlayerWallet>() : null;
            if (wallet != null) wallet.Add(coinReward);
            PlayerForgeSystem forge = player != null ? player.GetComponent<PlayerForgeSystem>() : null;
            int materialReward = (4 + currentWave * 2) * ((int)selectedDifficulty + 1);
            if (forge != null) forge.AddMaterials(materialReward);
            SetStatus("DUNGEON CLEARED!  +" + coinReward + " COINS  +" + materialReward + " FORGE MATERIALS");
            RefreshSelectionLabel();
        }

        private void BuildSelectionUI()
        {
            if (statusLabel == null) return;
            Transform parent = statusLabel.transform.parent;
            themeButtons[0] = CreateSelectionButton(parent, "ASHEN", new Vector2(-315f, -165f),
                () => SelectTheme(DungeonTheme.AshenRuins));
            themeButtons[1] = CreateSelectionButton(parent, "FOREST", new Vector2(-105f, -165f),
                () => SelectTheme(DungeonTheme.VerdantForest));
            themeButtons[2] = CreateSelectionButton(parent, "THUNDER", new Vector2(105f, -165f),
                () => SelectTheme(DungeonTheme.ThunderShrine));
            themeButtons[3] = CreateSelectionButton(parent, "TEMPLE", new Vector2(315f, -165f),
                () => SelectTheme(DungeonTheme.DrownedTemple));
            difficultyButton = CreateSelectionButton(parent, "DIFFICULTY: NORMAL",
                new Vector2(0f, -220f), CycleDifficulty);
        }

        private Button CreateSelectionButton(Transform parent, string label, Vector2 position,
            UnityEngine.Events.UnityAction action)
        {
            GameObject obj = new GameObject(label + " Selector", typeof(RectTransform), typeof(Image), typeof(Button));
            obj.transform.SetParent(parent, false);
            RectTransform rect = obj.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(190f, 46f);
            Image image = obj.GetComponent<Image>();
            image.color = new Color(0.1f, 0.24f, 0.34f, 0.94f);
            Button button = obj.GetComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(action);
            GameObject textObject = new GameObject("Label", typeof(RectTransform), typeof(Text));
            textObject.transform.SetParent(obj.transform, false);
            RectTransform textRect = textObject.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            Text text = textObject.GetComponent<Text>();
            text.text = label;
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.fontSize = 17;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            text.raycastTarget = false;
            return button;
        }

        private int RequiredLevelForTheme(DungeonTheme theme)
        {
            switch (theme)
            {
                case DungeonTheme.VerdantForest: return 10;
                case DungeonTheme.ThunderShrine: return 18;
                case DungeonTheme.DrownedTemple: return 28;
                default: return 5;
            }
        }

        private bool IsThemeUnlocked(DungeonTheme theme)
        {
            return GetPlayerLevel() >= RequiredLevelForTheme(theme);
        }

        private void SelectTheme(DungeonTheme theme)
        {
            if (running) return;
            if (!IsThemeUnlocked(theme))
            {
                SetStatus("REGION LOCKED  |  REQUIRED LEVEL " + RequiredLevelForTheme(theme));
                return;
            }
            selectedTheme = theme;
            RefreshSelectionLabel();
            SetStatus(ThemeName() + " READY  |  " + DifficultyName());
        }

        private void CycleDifficulty()
        {
            if (running) return;
            selectedDifficulty = (DungeonDifficulty)(((int)selectedDifficulty + 1) % 3);
            RefreshSelectionLabel();
            SetStatus(ThemeName() + " READY  |  " + DifficultyName());
        }

        private void RefreshSelectionLabel()
        {
            if (difficultyButton == null) return;
            Text label = difficultyButton.GetComponentInChildren<Text>();
            if (label != null) label.text = "DIFFICULTY: " + DifficultyName();
            difficultyButton.interactable = !running;
            DungeonTheme[] themes =
            {
                DungeonTheme.AshenRuins, DungeonTheme.VerdantForest,
                DungeonTheme.ThunderShrine, DungeonTheme.DrownedTemple
            };
            string[] labels = { "ASHEN", "FOREST", "THUNDER", "TEMPLE" };
            for (int i = 0; i < themeButtons.Length; i++)
            {
                if (themeButtons[i] == null) continue;
                bool unlocked = IsThemeUnlocked(themes[i]);
                themeButtons[i].interactable = !running && unlocked;
                Text themeLabel = themeButtons[i].GetComponentInChildren<Text>();
                if (themeLabel != null)
                    themeLabel.text = unlocked ? labels[i] : labels[i] + "  LV " + RequiredLevelForTheme(themes[i]);
            }
        }

        private void SetStatus(string value)
        {
            if (statusLabel != null) statusLabel.text = value;
        }
    }
}