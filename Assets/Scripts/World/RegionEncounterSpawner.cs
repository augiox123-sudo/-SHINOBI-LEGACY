using System.Collections.Generic;
using UnityEngine;
using ShinobiLegacy.Combat;
using ShinobiLegacy.Enemies;
using ShinobiLegacy.Items;
using ShinobiLegacy.Player;

namespace ShinobiLegacy.World
{
    /// <summary>
    /// Lightweight regional patrol encounter. Spawns a small group when the player
    /// enters an unlocked region and respawns after the area has been cleared.
    /// </summary>
    [RequireComponent(typeof(SphereCollider), typeof(Rigidbody))]
    public sealed class RegionEncounterSpawner : MonoBehaviour
    {
        [SerializeField] private string regionId;
        [SerializeField] private string enemyFaction = "Rogue Shinobi";
        [SerializeField] private Color enemyColor = new Color(0.55f, 0.16f, 0.14f);
        [SerializeField, Min(1)] private int requiredLevel = 1;
        [SerializeField, Min(1)] private int patrolSize = 3;
        [SerializeField, Min(10f)] private float respawnDelay = 30f;

        private readonly List<GameObject> activeEnemies = new List<GameObject>();
        private Transform player;
        private bool encounterStarted;
        private bool championActive;
        private float respawnAt;

        public void Configure(string id, string faction, Color color, int level, int count)
        {
            regionId = id;
            enemyFaction = faction;
            enemyColor = color;
            requiredLevel = Mathf.Max(1, level);
            patrolSize = Mathf.Clamp(count, 1, 5);

            SphereCollider area = GetComponent<SphereCollider>();
            area.isTrigger = true;
            area.radius = 42f;
            Rigidbody body = GetComponent<Rigidbody>();
            body.isKinematic = true;
            body.useGravity = false;
        }

        private void OnTriggerEnter(Collider other)
        {
            PlayerProgression progression = other.GetComponent<PlayerProgression>();
            if (progression == null) progression = other.GetComponentInParent<PlayerProgression>();
            if (progression == null || progression.Level < requiredLevel) return;

            WorldRegionProgression regions = progression.GetComponent<WorldRegionProgression>();
            if (regions != null && !regions.IsUnlocked(regionId)) return;

            player = progression.transform;
            if (!encounterStarted) SpawnPatrol();
        }

        private void Update()
        {
            activeEnemies.RemoveAll(enemy => enemy == null);
            if (!encounterStarted || activeEnemies.Count > 0 || player == null) return;
            if (Time.time < respawnAt) return;

            float distance = Vector3.Distance(player.position, transform.position);
            if (distance <= 48f) SpawnPatrol();
            else encounterStarted = false;
        }

        private void SpawnPatrol()
        {
            if (player == null) return;
            encounterStarted = true;
            championActive = false;
            activeEnemies.Clear();

            PlayerProgression progression = player.GetComponent<PlayerProgression>();
            int level = progression != null ? progression.Level : requiredLevel;
            for (int i = 0; i < patrolSize; i++)
            {
                float angle = i * Mathf.PI * 2f / patrolSize;
                Vector3 offset = new Vector3(Mathf.Cos(angle) * 12f, 1f, Mathf.Sin(angle) * 12f);
                GameObject enemy = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                enemy.name = regionId + "_" + enemyFaction.Replace(" ", "_") + "_" + (i + 1);
                enemy.transform.position = transform.position + offset;
                Renderer renderer = enemy.GetComponent<Renderer>();
                Shader shader = Shader.Find("Universal Render Pipeline/Lit");
                if (shader == null) shader = Shader.Find("Standard");
                Material material = new Material(shader) { color = enemyColor };
                renderer.material = material;
                Object.Destroy(enemy.GetComponent<CapsuleCollider>());

                CharacterController controller = enemy.AddComponent<CharacterController>();
                controller.height = 2f;
                controller.radius = 0.42f;
                CombatantHealth health = enemy.AddComponent<CombatantHealth>();
                health.SetMaxHealth(100f + level * 22f);
                EnemyCombatAI ai = enemy.AddComponent<EnemyCombatAI>();
                ai.Configure(2.2f + level * 0.025f, 10f + level * 2.2f, 1.5f, 24f + level * 2f);
                ai.SetTarget(player);

                EnemyRewardDrop rewards = enemy.AddComponent<EnemyRewardDrop>();
                rewards.Configure(progression, 28 + level * 8, 0.72f, false);
                health.Died += OnPatrolEnemyDied;
                activeEnemies.Add(enemy);
            }
            Debug.Log("SHINOBI LEGACY: " + enemyFaction + " patrol engaged in " + regionId);
        }

        private void OnPatrolEnemyDied()
        {
            RegionalBountySystem bounty = player != null ? player.GetComponent<RegionalBountySystem>() : null;
            if (bounty != null) bounty.RegisterKill(regionId);

            activeEnemies.RemoveAll(enemy => enemy == null ||
                enemy.GetComponent<CombatantHealth>() == null ||
                enemy.GetComponent<CombatantHealth>().IsDead);
            if (activeEnemies.Count > 0) return;

            if (!championActive)
            {
                SpawnRegionalChampion();
                return;
            }

            respawnAt = Time.time + respawnDelay;
            Debug.Log("SHINOBI LEGACY: Regional champion defeated in " + regionId);
        }

        private void SpawnRegionalChampion()
        {
            championActive = true;
            Vector3 position = transform.position + new Vector3(0f, 1.4f, 25f);
            GameObject champion = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            champion.name = regionId + "_Elite_Champion";
            champion.transform.position = position;
            champion.transform.localScale = new Vector3(1.45f, 1.5f, 1.45f);

            Renderer renderer = champion.GetComponent<Renderer>();
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            renderer.material = new Material(shader) { color = Color.Lerp(enemyColor, Color.yellow, 0.42f) };
            Object.Destroy(champion.GetComponent<CapsuleCollider>());

            CharacterController controller = champion.AddComponent<CharacterController>();
            controller.height = 2.4f;
            controller.radius = 0.58f;
            CombatantHealth health = champion.AddComponent<CombatantHealth>();
            PlayerProgression progression = player != null ? player.GetComponent<PlayerProgression>() : null;
            int level = progression != null ? progression.Level : requiredLevel;
            health.SetMaxHealth(650f + level * 95f);

            BossCombatAI boss = champion.AddComponent<BossCombatAI>();
            boss.SetTarget(player);
            int profile = regionId == "verdant_forest" ? 1 :
                regionId == "thunder_shrine" ? 2 :
                regionId == "drowned_temple" ? 3 : 0;
            boss.ConfigureProfile(profile);

            EnemyRewardDrop rewards = champion.AddComponent<EnemyRewardDrop>();
            rewards.Configure(progression, 180 + level * 30, 1f, true);
            rewards.SetLegendaryChance(0.12f);
            health.Died += OnPatrolEnemyDied;
            activeEnemies.Add(champion);
            Debug.Log("SHINOBI LEGACY: Elite champion appeared in " + regionId);
        }
    }
}
