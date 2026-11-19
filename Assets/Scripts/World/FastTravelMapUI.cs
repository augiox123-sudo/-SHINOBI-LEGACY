using System.Collections.Generic;
using UnityEngine;
using ShinobiLegacy.Combat;
using ShinobiLegacy.Items;

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
            if (GUI.Button(new Rect(Screen.width - buttonWidth - 22, 22, buttonWidth, buttonHeight), "WORLD MAP"))
                mapOpen = !mapOpen;

            if (!mapOpen) return;

            float width = Mathf.Min(560f, Screen.width - 32f);
            float height = Mathf.Min(650f, Screen.height - 48f);
            Rect panel = new Rect((Screen.width - width) * 0.5f, (Screen.height - height) * 0.5f, width, height);
            GUI.Box(panel, "SHINOBI WORLD MAP  |  FAST TRAVEL");
            if (GUI.Button(new Rect(panel.xMax - 88f, panel.y + 8f, 76f, 34f), "CLOSE"))
                mapOpen = false;

            GUILayout.BeginArea(new Rect(panel.x + 18f, panel.y + 54f, panel.width - 36f, panel.height - 70f));
            GUILayout.Label("Unlocked travel points");
            scroll = GUILayout.BeginScrollView(scroll);
            FastTravelPoint[] points = FindObjectsOfType<FastTravelPoint>();
            bool found = false;
            foreach (FastTravelPoint point in points)
            {
                if (point == null || !point.IsUnlocked) continue;
                found = true;
                GUILayout.BeginHorizontal("box");
                GUILayout.Label(point.displayName, GUILayout.ExpandWidth(true), GUILayout.Height(42f));
                if (GUILayout.Button("TRAVEL", GUILayout.Width(112f), GUILayout.Height(42f)))
                {
                    TravelTo(point);
                    mapOpen = false;
                }
                GUILayout.EndHorizontal();
            }
            if (!found) GUILayout.Label("No travel points discovered yet. Explore the village to unlock them.");
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
