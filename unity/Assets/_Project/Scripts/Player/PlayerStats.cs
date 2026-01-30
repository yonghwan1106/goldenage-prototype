using UnityEngine;
using System;
using GoldenAge.Core;

namespace GoldenAge.Player
{
    /// <summary>
    /// 플레이어의 스탯(체력, 에너지, 레벨, 경험치)을 관리하는 클래스
    /// </summary>
    public class PlayerStats : MonoBehaviour
    {
        [Header("기본 스탯")]
        [SerializeField] private int maxHealth = 100;
        [SerializeField] private int maxEnergy = 100;

        [Header("레벨 시스템")]
        [SerializeField] private int level = 1;
        [SerializeField] private int experience = 0;
        [SerializeField] private int experienceToNextLevel = 100;
        [SerializeField] private float expMultiplier = 1.5f;

        [Header("레벨업 보너스")]
        [SerializeField] private int healthPerLevel = 10;
        [SerializeField] private int energyPerLevel = 5;

        private int currentHealth;
        private int currentEnergy;

        // Properties
        public int CurrentHealth => currentHealth;
        public int MaxHealth => maxHealth;
        public int CurrentEnergy => currentEnergy;
        public int MaxEnergy => maxEnergy;
        public int Level => level;
        public int Experience => experience;
        public int ExperienceToNextLevel => experienceToNextLevel;

        public float HealthPercent => maxHealth > 0 ? (float)currentHealth / maxHealth : 0f;
        public float EnergyPercent => maxEnergy > 0 ? (float)currentEnergy / maxEnergy : 0f;
        public float ExpPercent => experienceToNextLevel > 0 ? (float)experience / experienceToNextLevel : 0f;

        // Events
        public event Action<int, int> OnHealthChanged;      // current, max
        public event Action<int, int> OnEnergyChanged;      // current, max
        public event Action<int> OnLevelUp;                 // new level
        public event Action<int> OnExperienceGained;        // amount
        public event Action OnPlayerDeath;

        private void Awake()
        {
            currentHealth = maxHealth;
            currentEnergy = maxEnergy;
        }

        private void Start()
        {
            // 초기 이벤트 발생
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
            OnEnergyChanged?.Invoke(currentEnergy, maxEnergy);
        }

        #region Health

        /// <summary>
        /// 데미지 받기
        /// </summary>
        public void TakeDamage(int damage)
        {
            if (damage <= 0) return;

            currentHealth = Mathf.Max(0, currentHealth - damage);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);

            Debug.Log($"[PlayerStats] Took {damage} damage. Health: {currentHealth}/{maxHealth}");

            if (currentHealth <= 0)
            {
                Die();
            }
        }

        /// <summary>
        /// 회복
        /// </summary>
        public void Heal(int amount)
        {
            if (amount <= 0) return;

            int previousHealth = currentHealth;
            currentHealth = Mathf.Min(maxHealth, currentHealth + amount);

            if (currentHealth != previousHealth)
            {
                OnHealthChanged?.Invoke(currentHealth, maxHealth);
                Debug.Log($"[PlayerStats] Healed {currentHealth - previousHealth}. Health: {currentHealth}/{maxHealth}");
            }
        }

        /// <summary>
        /// 체력 설정 (저장/불러오기용)
        /// </summary>
        public void SetHealth(int current, int max)
        {
            maxHealth = max;
            currentHealth = Mathf.Clamp(current, 0, max);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }

        #endregion

        #region Energy

        /// <summary>
        /// 에너지 회복
        /// </summary>
        public void RestoreEnergy(int amount)
        {
            if (amount <= 0) return;

            int previousEnergy = currentEnergy;
            currentEnergy = Mathf.Min(maxEnergy, currentEnergy + amount);

            if (currentEnergy != previousEnergy)
            {
                OnEnergyChanged?.Invoke(currentEnergy, maxEnergy);
            }
        }

        /// <summary>
        /// 에너지 사용 (성공 여부 반환)
        /// </summary>
        public bool UseEnergy(int amount)
        {
            if (currentEnergy < amount) return false;

            currentEnergy -= amount;
            OnEnergyChanged?.Invoke(currentEnergy, maxEnergy);
            return true;
        }

        /// <summary>
        /// 에너지가 충분한지 확인
        /// </summary>
        public bool HasEnergy(int amount)
        {
            return currentEnergy >= amount;
        }

        #endregion

        #region Experience & Level

        /// <summary>
        /// 경험치 획득
        /// </summary>
        public void AddExperience(int exp)
        {
            if (exp <= 0) return;

            experience += exp;
            OnExperienceGained?.Invoke(exp);

            Debug.Log($"[PlayerStats] Gained {exp} EXP. Total: {experience}/{experienceToNextLevel}");

            while (experience >= experienceToNextLevel)
            {
                experience -= experienceToNextLevel;
                LevelUp();
            }
        }

        private void LevelUp()
        {
            level++;
            experienceToNextLevel = Mathf.RoundToInt(experienceToNextLevel * expMultiplier);

            // 레벨업 보너스
            maxHealth += healthPerLevel;
            maxEnergy += energyPerLevel;

            // 체력/에너지 완전 회복
            currentHealth = maxHealth;
            currentEnergy = maxEnergy;

            OnHealthChanged?.Invoke(currentHealth, maxHealth);
            OnEnergyChanged?.Invoke(currentEnergy, maxEnergy);
            OnLevelUp?.Invoke(level);

            Debug.Log($"[PlayerStats] Level Up! Now level {level}. Next level at {experienceToNextLevel} EXP");
        }

        /// <summary>
        /// 레벨 설정 (저장/불러오기용)
        /// </summary>
        public void SetLevel(int newLevel, int newExp)
        {
            level = newLevel;
            experience = newExp;

            // 레벨에 따른 스탯 재계산
            maxHealth = 100 + (level - 1) * healthPerLevel;
            maxEnergy = 100 + (level - 1) * energyPerLevel;
        }

        #endregion

        private void Die()
        {
            Debug.Log("[PlayerStats] Player died!");
            OnPlayerDeath?.Invoke();
            GameManager.Instance?.ChangeState(GameState.Paused);
        }

        #region Debug

        [ContextMenu("Debug: Full Heal")]
        private void DebugFullHeal()
        {
            currentHealth = maxHealth;
            currentEnergy = maxEnergy;
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
            OnEnergyChanged?.Invoke(currentEnergy, maxEnergy);
        }

        [ContextMenu("Debug: Add 100 EXP")]
        private void DebugAddExp()
        {
            AddExperience(100);
        }

        [ContextMenu("Debug: Take 20 Damage")]
        private void DebugTakeDamage()
        {
            TakeDamage(20);
        }

        #endregion
    }
}
