using System;
using UnityEngine;

namespace ShinobiLegacy.Player
{
    public sealed class PlayerProgression : MonoBehaviour
    {
        [SerializeField] private int startingLevel = 1;
        [SerializeField] private int baseExperienceToNextLevel = 100;
        [SerializeField] private float experienceGrowth = 1.35f;
        [SerializeField] private int levelUpHealthBonus = 12;
        public int Level { get; private set; }
        public int Experience { get; private set; }
        public int ExperienceToNextLevel => Mathf.RoundToInt(baseExperienceToNextLevel * Mathf.Pow(experienceGrowth, Mathf.Max(0, Level - 1)));
        public event Action<int, int, int> ProgressChanged;
        public event Action<int> LevelUp;

        private void Awake()
        {
            Level = Mathf.Max(1, startingLevel);
            Experience = 0;
        }

        public void RestoreProgress(int savedLevel, int savedExperience)
        {
            Level = Mathf.Max(1, savedLevel);
            Experience = Mathf.Max(0, savedExperience);
            Combat.CombatantHealth health = GetComponent<Combat.CombatantHealth>();
            if (health != null && Level > 1)
                health.SetMaxHealth(health.MaxHealth + (Level - 1) * levelUpHealthBonus);
            ProgressChanged?.Invoke(Level, Experience, ExperienceToNextLevel);
        }

        public void AddExperience(int amount)
        {
            if (amount <= 0) return;
            Experience += amount;
            while (Experience >= ExperienceToNextLevel)
            {
                Experience -= ExperienceToNextLevel;
                Level++;
                Combat.CombatantHealth health = GetComponent<Combat.CombatantHealth>();
                if (health != null) health.SetMaxHealth(health.MaxHealth + levelUpHealthBonus);
                LevelUp?.Invoke(Level);
            }
            ProgressChanged?.Invoke(Level, Experience, ExperienceToNextLevel);
        }
    }
}