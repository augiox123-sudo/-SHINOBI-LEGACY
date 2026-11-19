using UnityEngine;
using UnityEngine.UI;
using ShinobiLegacy.Combat;
using ShinobiLegacy.Items;
using ShinobiLegacy.Player;

namespace ShinobiLegacy.Enemies
{
    /// <summary>
    /// Scalable dungeon run: enemy waves, a dedicated final boss and completion rewards.
    /// </summary>
    public sealed class DungeonWaveController : MonoBehaviour
    {
        [SerializeField] private int totalWaves = 3;
        [SerializeField] private int enemiesPerWave = 3;
        [SerializeField] private Transform player;
        [SerializeField] private Text statusLabel;
        [SerializeField] private int completionCoinReward = 250;
        private int currentWave;
        private int alive;
        private float nextWaveTime;
        private bool running;
        private bool waitingForNextWave;
        private bool completed;

        public bool IsRunning => running;
        public int CurrentWave => currentWave;
        public int TotalWaves => totalWaves;

        public void Configure(Transform playerTransform, Text label, int waves, int enemies)
        {
            player = playerTransform;
            statusLabel = label;
            totalWaves = Mathf.Max(1, waves);
            enemiesPerWave = Mathf.Max(1, enemies);
            SetStatus("DUNGEON READY");
        }

        public void StartDungeon()
        {
            if (running || player == null) return;
            running = true;
            completed = false;
            waitingForNextWave = false;
            currentWave = 0;
            SpawnNextWave();
        }

        private void Update()
        {
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
            SetStatus("WAVE " + currentWave + "/" + totalWaves +
                (bossWave ? "  BOSS APPROACHING" : "  ENEMIES " + alive));

            for (int i = 0; i < regularCount; i++)
                SpawnEnemy(i, regularCount, false);

            if (bossWave) SpawnBoss(regularCount);
        }

        private void SpawnEnemy(int index, int count, bool elite)
        {
            float angle = (Mathf.PI * 2f * index) / Mathf.Max(1, count);
            Vector3 position = player.position + new Vector3(Mathf.Cos(angle) * 8f, 1f, Mathf.Sin(angle) * 8f);
            GameObject enemy = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            enemy.name = "Dungeon_W" + currentWave + "_Enemy_" + (index + 1);
            enemy.transform.position = position;
            Renderer renderer = enemy.GetComponent<Renderer>();
            Material material = CreateMaterial();
            material.color = elite ? new Color(0.68f, 0.28f, 0.08f) : new Color(0.48f, 0.12f, 0.18f);
            renderer.material = material;
            Object.Destroy(enemy.GetComponent<CapsuleCollider>());

            CharacterController controller = enemy.AddComponent<CharacterController>();
            controller.height = 2f;
            controller.radius = 0.42f;
            controller.center = Vector3.zero;

            int level = GetPlayerLevel();
            CombatantHealth health = enemy.AddComponent<CombatantHealth>();
            health.SetMaxHealth((85f + currentWave * 35f + level * 8f) * (elite ? 1.7f : 1f));
            EnemyCombatAI ai = enemy.AddComponent<EnemyCombatAI>();
            ai.Configure(2.1f + currentWave * 0.18f, 8f + currentWave * 3f, 1.7f,
                20f + currentWave * 2f + level);
            ai.SetTarget(player);
            EnemyRewardDrop rewards = enemy.AddComponent<EnemyRewardDrop>();
            rewards.Configure(player.GetComponent<PlayerProgression>(),
                25 + currentWave * 15 + level * 3, elite ? 0.95f : 0.7f + currentWave * 0.08f, elite);
            health.Died += OnEnemyDied;
        }

        private void SpawnBoss(int index)
        {
            Vector3 position = player.position + new Vector3(0f, 1.5f, 13f);
            GameObject boss = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            boss.name = "Dungeon_Final_Boss_Warden";
            boss.transform.position = position;
            boss.transform.localScale = new Vector3(1.7f, 1.65f, 1.7f);
            Renderer renderer = boss.GetComponent<Renderer>();
            Material material = CreateMaterial();
            material.color = new Color(0.62f, 0.12f, 0.055f);
            renderer.material = material;
            Object.Destroy(boss.GetComponent<CapsuleCollider>());

            CharacterController controller = boss.AddComponent<CharacterController>();
            controller.height = 2.4f;
            controller.radius = 0.62f;
            controller.center = Vector3.zero;

            int level = GetPlayerLevel();
            CombatantHealth health = boss.AddComponent<CombatantHealth>();
            health.SetMaxHealth(650f + currentWave * 180f + level * 45f);
            BossCombatAI bossAI = boss.AddComponent<BossCombatAI>();
            bossAI.SetTarget(player);

            EnemyRewardDrop rewards = boss.AddComponent<EnemyRewardDrop>();
            rewards.Configure(player.GetComponent<PlayerProgression>(),
                250 + currentWave * 100 + level * 20, 1f, true);
            health.Died += OnEnemyDied;
            SetStatus("BOSS: THE ASHEN WARDEN  |  DEFEAT IT!");
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

        private void OnEnemyDied()
        {
            if (!running) return;
            alive = Mathf.Max(0, alive - 1);
            SetStatus("WAVE " + currentWave + "/" + totalWaves + "   ENEMIES " + alive);
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
            PlayerWallet wallet = player != null ? player.GetComponent<PlayerWallet>() : null;
            if (wallet != null) wallet.Add(completionCoinReward + currentWave * 75);
            SetStatus("DUNGEON CLEARED!  REWARD +" + (completionCoinReward + currentWave * 75) + " COINS");
        }

        private void SetStatus(string value)
        {
            if (statusLabel != null) statusLabel.text = value;
        }
    }
}
