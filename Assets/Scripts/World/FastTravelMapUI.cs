using System.Collections.Generic;
using UnityEngine;
using ShinobiLegacy.Combat;
using ShinobiLegacy.Items;
using ShinobiLegacy.Player;

namespace ShinobiLegacy.World
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class FastTravelMapUI : MonoBehaviour
    {
        private bool mapOpen;
        private Vector2 scroll;
        private CharacterController controller;
        private PlayerSaveSystem saveSystem;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            saveSystem = GetComponent<PlayerSaveSystem>();
        }

        private void OnGUI()
        {
            const int buttonWidth = 150;
            const int buttonHeight = 52;
            if (GUI.Button(new Rect(22, 22, buttonWidth, buttonHeight), "WORLD MAP"))
                mapOpen = !mapOpen;

            if (!mapOpen) return;

            float width = Mathf.Min(680f, Screen.width - 32f);
            float height = Mathf.Min(720f, Screen.height - 36f);
            Rect panel = new Rect((Screen.width - width) * 0.5f, (Screen.height - height) * 0.5f, width, height);
            GUI.Box(panel, "SHINOBI WORLD MAP  |  FAST TRAVEL");
            if (GUI.Button(new Rect(panel.xMax - 88f, panel.y + 8f, 76f, 34f), "CLOSE"))
                mapOpen = false;

            Rect mapRect = new Rect(panel.x + 18f, panel.y + 52f, panel.width - 36f, Mathf.Min(390f, panel.height * 0.55f));
            GUI.Box(mapRect, "WORLD REGIONS  |  VILLAGE + 4 EXPEDITION ZONES");

            // Stylized village streets and central market square.
            GUI.color = new Color(0.62f, 0.54f, 0.38f, 0.9f);
            GUI.Box(new Rect(mapRect.center.x - 5f, mapRect.y + 24f, 10f, mapRect.height - 48f), GUIContent.none);
            GUI.Box(new Rect(mapRect.x + 18f, mapRect.center.y - 5f, mapRect.width - 36f, 10f), GUIContent.none);
            GUI.color = Color.white;

            FastTravelPoint[] points = FindObjectsOfType<FastTravelPoint>();
            foreach (FastTravelPoint point in points)
            {
                if (point == null || !point.IsUnlocked) continue;
                Vector3 position = point.transform.position;
                float mapX = mapRect.x + 16f + Mathf.InverseLerp(-250f, 250f, position.x) * (mapRect.width - 32f);
                float mapY = mapRect.y + 34f + (1f - Mathf.InverseLerp(-250f, 250f, position.z)) * (mapRect.height - 68f);
                Rect marker = new Rect(mapX - 12f, mapY - 12f, 24f, 24f);
                if (GUI.Button(marker, "◆"))
                {
                    TravelTo(point);
                    mapOpen = false;
                }
                GUI.Label(new Rect(mapX - 62f, mapY + 13f, 124f, 28f), point.displayName);
            }

            GUILayout.BeginArea(new Rect(panel.x + 18f, mapRect.yMax + 12f, panel.width - 36f, panel.yMax - mapRect.yMax - 24f));
            scroll = GUILayout.BeginScrollView(scroll);
            GUILayout.Label("REGIONAL ACCESS");
            WorldRegionProgression regionProgression = GetComponent<WorldRegionProgression>();
            PlayerProgression playerProgression = GetComponent<PlayerProgression>();
            int playerLevel = playerProgression != null ? playerProgression.Level : 1;
            foreach (WorldRegionProgression.Region region in WorldRegionProgression.Regions)
            {
                bool unlocked = regionProgression != null && regionProgression.IsUnlocked(region.id);
                GUILayout.BeginHorizontal("box");
                int levelsRemaining = regionProgression != null ? regionProgression.GetLevelsUntilUnlock(region.id) :
                    Mathf.Max(0, region.requiredLevel - playerLevel);
                string access = unlocked ? "ACCESS: UNLOCKED" :
                    "LOCKED - " + levelsRemaining + " LEVEL(S) TO GO";
                GUILayout.Label((unlocked ? "[UNLOCKED] " : "[LOCKED] ") + region.displayName +
                    "\n" + region.description +
                    "\n" + access + "  |  RECOMMENDED LEVEL " + region.requiredLevel +
                    "\nDUNGEON: " + region.dungeonName + "  |  BOSS: " + region.bossName,
                    GUILayout.ExpandWidth(true), GUILayout.Height(82f));
                GUILayout.EndHorizontal();
            }

            GUILayout.Space(8f);
            GUILayout.Space(8f);
            GUILayout.Label("DISCOVERED DESTINATIONS  |  CHARACTER LEVEL " + playerLevel);
            bool found = false;
            foreach (FastTravelPoint point in points)
            {
                if (point == null || !point.IsUnlocked) continue;
                found = true;
                GUILayout.BeginHorizontal("box");
                GUILayout.Label(point.displayName, GUILayout.ExpandWidth(true), GUILayout.Height(40f));
                if (GUILayout.Button("TRAVEL", GUILayout.Width(112f), GUILayout.Height(40f)))
                {
                    TravelTo(point);
                    mapOpen = false;
                }
                GUILayout.EndHorizontal();
            }
            if (!found) GUILayout.Label("Explore the world to discover travel points.");
            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        private void TravelTo(FastTravelPoint point)
        {
            if (point == null) return;
            if (controller != null) controller.enabled = false;
            transform.position = point.transform.position + Vector3.up * 1.2f;
            if (controller != null) controller.enabled = true;
            CombatantHealth health = GetComponent<CombatantHealth>();
            if (health != null && health.IsDead) health.ReviveAtFullHealth(2f);
            if (saveSystem != null) saveSystem.Save();
        }
    }
}
