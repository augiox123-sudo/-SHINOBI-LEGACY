using UnityEngine;
using UnityEngine.UI;
using ShinobiLegacy.Combat;
using ShinobiLegacy.Items;

namespace ShinobiLegacy.Enemies
{
    public sealed class DungeonWaveController : MonoBehaviour
    {
        [SerializeField] private int totalWaves = 3;
        [SerializeField] private int enemiesPerWave = 3;
        [SerializeField] private Transform player;
        [SerializeField] private Text statusLabel;
        private int currentWave;
        private int alive;
        private float nextWaveTime;
        private bool running;
        private bool waitingForNextWave;

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
            if (running) return;
            running = true;
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
                running = false;
                SetStatus("DUNGEON CLEARED!");
                return;
            }

            alive = enemiesPerWave + currentWave - 1;
            SetStatus("WAVE " + currentWave + "/" + totalWaves + "   ENEMIES " + alive);
            for (int i = 0; i < alive; i++) SpawnEnemy(i);
        }

        private void SpawnEnemy(int index)
        {
            float angle = (Mathf.PI * 2f * index) / Mathf.Max(1, alive);
            Vector3 position = player.position + new Vector3(Mathf.Cos(angle) * 8f, 1f, Mathf.Sin(angle) * 8f);
            GameObject enemy = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            enemy.name = "Dungeon_W" + currentWave + "_Enemy_" + (index + 1);
            enemy.transform.position = position;
            Renderer renderer = enemy.GetComponent<Renderer>();
            Material material = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            material.color = currentWave == totalWaves ? new Color(0.55f, 0.18f, 0.08f) : new Color(0.48f, 0.12f, 0.18f);
            renderer.material = material;
            Object.Destroy(enemy.GetComponent<CapsuleCollider>());
            CharacterController controller = enemy.AddComponent<CharacterController>();
            controller.height = 2f;
            controller.radius = 0.42f;
            controller.center = Vector3.zero;

            CombatantHealth health = enemy.AddComponent<CombatantHealth>();
            health.SetMaxHealth(85f + currentWave * 35f);
            EnemyCombatAI ai = enemy.AddComponent<EnemyCombatAI>();
            ai.Configure(2.1f + currentWave * 0.18f, 8f + currentWave * 3f, 1.7f, 20f);
            ai.SetTarget(player);
            EnemyRewardDrop rewards = enemy.AddComponent<EnemyRewardDrop>();
            PlayerProgression progression = player.GetComponent<PlayerProgression>();
            rewards.Configure(progression, 25 + currentWave * 15, 0.7f + currentWave * 0.08f, currentWave == totalWaves);
            health.Died += OnEnemyDied;
        }

        private void OnEnemyDied()
        {
            alive = Mathf.Max(0, alive - 1);
            SetStatus("WAVE " + currentWave + "/" + totalWaves + "   ENEMIES " + alive);
            if (alive == 0)
            {
                if (currentWave >= totalWaves)
                {
                    running = false;
                    SetStatus("DUNGEON CLEARED!");
                }
                else
                {
                    waitingForNextWave = true;
                    nextWaveTime = Time.time + 2.5f;
                    SetStatus("WAVE CLEARED - NEXT WAVE INCOMING");
                }
            }
        }

        private void SetStatus(string value)
        {
            if (statusLabel != null) statusLabel.text = value;
        }
    }
}