using System.Collections.Generic;
using UnityEngine;

namespace ShinobiLegacy.Combat
{
    /// <summary>
    /// Lightweight mobile-friendly target lock. It scans with a fixed collider buffer,
    /// keeps a valid target selected, and lets the player cycle nearby enemies.
    /// </summary>
    public sealed class EnemyTargetLockSystem : MonoBehaviour
    {
        [SerializeField, Min(3f)] private float acquireRange = 18f;
        [SerializeField, Range(-1f, 1f)] private float minimumForwardDot = -0.15f;
        [SerializeField, Min(0.05f)] private float scanInterval = 0.25f;
        [SerializeField] private bool showTargetMarker = true;

        private readonly Collider[] scanBuffer = new Collider[64];
        private readonly List<CombatantHealth> candidates = new List<CombatantHealth>(16);
        private CombatantHealth currentTarget;
        private float nextScanTime;
        private Camera worldCamera;

        public CombatantHealth CurrentTarget => currentTarget;

        private void Awake()
        {
            worldCamera = Camera.main;
        }

        private void Update()
        {
            if (Time.unscaledTime < nextScanTime && IsTargetValid()) return;
            nextScanTime = Time.unscaledTime + Mathf.Max(0.05f, scanInterval);
            if (!IsTargetValid()) AcquireBestTarget();
        }

        public void CycleTarget()
        {
            CollectCandidates();
            if (candidates.Count == 0)
            {
                currentTarget = null;
                return;
            }

            candidates.Sort((a, b) =>
                (a.transform.position - transform.position).sqrMagnitude.CompareTo(
                (b.transform.position - transform.position).sqrMagnitude));

            int currentIndex = candidates.IndexOf(currentTarget);
            currentTarget = candidates[(currentIndex + 1) % candidates.Count];
        }

        public void ClearTarget()
        {
            currentTarget = null;
        }

        private bool IsTargetValid()
        {
            if (currentTarget == null || currentTarget.IsDead) return false;
            Vector3 offset = currentTarget.transform.position - transform.position;
            offset.y = 0f;
            return offset.sqrMagnitude <= acquireRange * acquireRange;
        }

        private void AcquireBestTarget()
        {
            CollectCandidates();
            CombatantHealth best = null;
            float bestScore = float.MaxValue;
            for (int i = 0; i < candidates.Count; i++)
            {
                CombatantHealth candidate = candidates[i];
                Vector3 offset = candidate.transform.position - transform.position;
                offset.y = 0f;
                float distance = offset.sqrMagnitude;
                float facing = Vector3.Dot(transform.forward, offset.normalized);
                if (facing < minimumForwardDot) continue;
                // Prefer enemies near the center of the player's facing direction.
                float score = distance * (1f + (1f - facing) * 0.65f);
                if (score < bestScore)
                {
                    bestScore = score;
                    best = candidate;
                }
            }
            currentTarget = best;
        }

        private void CollectCandidates()
        {
            candidates.Clear();
            int count = Physics.OverlapSphereNonAlloc(transform.position, acquireRange, scanBuffer,
                Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < count; i++)
            {
                Collider hit = scanBuffer[i];
                if (hit != null)
                {
                    CombatantHealth health = hit.GetComponentInParent<CombatantHealth>();
                    if (health != null && health.transform != transform && !health.IsDead &&
                        !candidates.Contains(health))
                        candidates.Add(health);
                }
                scanBuffer[i] = null;
            }
        }

        private void OnGUI()
        {
            if (!IsTargetValid()) return;
            if (worldCamera == null) worldCamera = Camera.main;
            if (worldCamera == null) return;

            Vector3 worldPoint = currentTarget.transform.position + Vector3.up * 2.6f;
            Vector3 screenPoint = worldCamera.WorldToScreenPoint(worldPoint);
            if (screenPoint.z > 0f && showTargetMarker)
            {
                float x = screenPoint.x - 13f;
                float y = Screen.height - screenPoint.y - 13f;
                GUI.color = new Color(1f, 0.3f, 0.22f, 0.95f);
                GUI.Label(new Rect(x, y, 30f, 30f), "▼");
                GUI.color = Color.white;
            }

            float panelWidth = Mathf.Min(300f, Screen.width - 32f);
            Rect panel = new Rect((Screen.width - panelWidth) * 0.5f, 18f, panelWidth, 62f);
            GUI.Box(panel, currentTarget.name);
            float healthRatio = currentTarget.MaxHealth > 0f
                ? Mathf.Clamp01(currentTarget.CurrentHealth / currentTarget.MaxHealth) : 0f;
            GUI.Box(new Rect(panel.x + 12f, panel.y + 34f, panel.width - 24f, 13f), GUIContent.none);
            GUI.color = new Color(0.78f, 0.12f, 0.12f, 1f);
            GUI.DrawTexture(new Rect(panel.x + 14f, panel.y + 36f,
                (panel.width - 28f) * healthRatio, 9f), Texture2D.whiteTexture);
            GUI.color = Color.white;

            // Target cycling is handled by the shared Canvas combat controls.
            // Keep OnGUI focused on the target marker and health panel only.
        }
    }
}