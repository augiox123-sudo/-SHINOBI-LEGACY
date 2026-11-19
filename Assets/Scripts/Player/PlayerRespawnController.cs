using System.Collections;
using UnityEngine;
using ShinobiLegacy.Combat;
using ShinobiLegacy.Items;

namespace ShinobiLegacy.Player
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class PlayerRespawnController : MonoBehaviour
    {
        [SerializeField] private Transform safeSpawn;
        [SerializeField] private Vector3 fallbackSafePosition = new Vector3(0f, 1f, -58f);
        [SerializeField] private float respawnDelay = 2.5f;
        [SerializeField] private float protectionSeconds = 4f;

        private CharacterController controller;
        private CombatantHealth health;
        private PlayerSaveSystem saveSystem;
        private PlayerMotor motor;
        private bool respawning;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            health = GetComponent<CombatantHealth>();
            saveSystem = GetComponent<PlayerSaveSystem>();
            motor = GetComponent<PlayerMotor>();
        }

        private void OnEnable()
        {
            if (health == null) health = GetComponent<CombatantHealth>();
            if (health != null) health.Died += HandleDeath;
        }

        private void OnDisable()
        {
            if (health != null) health.Died -= HandleDeath;
        }

        public void SetSafeSpawn(Transform spawn) { safeSpawn = spawn; }

        private void HandleDeath()
        {
            if (!respawning) StartCoroutine(RespawnRoutine());
        }

        private IEnumerator RespawnRoutine()
        {
            respawning = true;
            if (saveSystem != null) saveSystem.Save();
            if (motor != null) motor.SetMovementEnabled(false);
            if (controller != null) controller.enabled = false;
            yield return new WaitForSeconds(respawnDelay);

            Vector3 destination = safeSpawn != null ? safeSpawn.position : fallbackSafePosition;
            transform.position = destination;
            transform.rotation = safeSpawn != null ? safeSpawn.rotation : Quaternion.identity;

            if (controller != null) controller.enabled = true;
            if (motor != null) motor.SetMovementEnabled(true);
            if (health != null) health.ReviveAtFullHealth(protectionSeconds);
            if (saveSystem != null) saveSystem.Save();
            respawning = false;
        }
    }
}
