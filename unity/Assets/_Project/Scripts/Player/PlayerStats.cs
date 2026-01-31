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

        [Header("자동 회복")]
        [SerializeField] private float energyRegenRate = 10f;  // 초당 에너지 회복량
        [SerializeField] private float energyRegenDelay = 1f;  // 스킬 사용 후 회복 시작까지 대기 시간

        [Header("재화")]
        [SerializeField] private int gold = 100;

        private int currentHealth;
        private int currentEnergy;
        private float energyRegenTimer;

        // 장비 보너스
        private int equipmentAttackBonus;
        private int equipmentDefenseBonus;
        private int equipmentHealthBonus;
        private int equipmentEnergyBonus;

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

        public int Gold => gold;
        public int AttackBonus => equipmentAttackBonus;
        public int DefenseBonus => equipmentDefenseBonus;

        // Events
        public event Action<int, int> OnHealthChanged;      // current, max
        public event Action<int, int> OnEnergyChanged;      // current, max
        public event Action<int> OnLevelUp;                 // new level
        public event Action<int> OnExperienceGained;        // amount
        public event Action OnPlayerDeath;
        public event Action OnDeath;                        // Alias for OnPlayerDeath
        public event Action<int> OnDamaged;                 // damage amount
        public event Action<int> OnGoldChanged;             // current gold

        public bool IsAlive => currentHealth > 0;

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

        private void Update()
        {
            // 에너지 자동 회복
            if (currentEnergy < maxEnergy)
            {
                if (energyRegenTimer > 0)
                {
                    energyRegenTimer -= Time.deltaTime;
                }
                else
                {
                    float regenAmount = energyRegenRate * Time.deltaTime;
                    currentEnergy = Mathf.Min(maxEnergy, currentEnergy + Mathf.RoundToInt(regenAmount));
                    OnEnergyChanged?.Invoke(currentEnergy, maxEnergy);
                }
            }
        }

        #region Health

        /// <summary>
        /// 데미지 받기
        /// </summary>
        public void TakeDamage(int damage)
        {
            if (damage <= 0) return;

            // 방어력으로 데미지 감소
            damage = ReduceDamage(damage);

            currentHealth = Mathf.Max(0, currentHealth - damage);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
            OnDamaged?.Invoke(damage);

            Debug.Log($"[PlayerStats] Took {damage} damage. Health: {currentHealth}/{maxHealth}");

            if (currentHealth <= 0)
            {
                Die();
            }
        }

        /// <summary>
        /// 데미지 받기 (타입 지정)
        /// </summary>
        public void TakeDamage(int damage, Combat.DamageType damageType)
        {
            // 특정 데미지 타입에 대한 추가 처리 가능
            TakeDamage(damage);
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

        /// <summary>
        /// 현재 체력만 설정
        /// </summary>
        public void SetHealth(float current)
        {
            currentHealth = Mathf.Clamp((int)current, 0, maxHealth);
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
            energyRegenTimer = energyRegenDelay;  // 회복 딜레이 리셋
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

        /// <summary>
        /// 에너지 설정 (저장/불러오기용)
        /// </summary>
        public void SetEnergy(int current, int max)
        {
            maxEnergy = max;
            currentEnergy = Mathf.Clamp(current, 0, max);
            OnEnergyChanged?.Invoke(currentEnergy, maxEnergy);
        }

        /// <summary>
        /// 현재 에너지만 설정
        /// </summary>
        public void SetEnergy(float current)
        {
            currentEnergy = Mathf.Clamp((int)current, 0, maxEnergy);
            OnEnergyChanged?.Invoke(currentEnergy, maxEnergy);
        }

        /// <summary>
        /// 에너지 사용 (float 오버로드)
        /// </summary>
        public bool UseEnergy(float amount)
        {
            return UseEnergy(Mathf.RoundToInt(amount));
        }

        /// <summary>
        /// 에너지 회복
        /// </summary>
        public void AddEnergy(int amount)
        {
            if (amount <= 0) return;
            currentEnergy = Mathf.Min(currentEnergy + amount, maxEnergy);
            OnEnergyChanged?.Invoke(currentEnergy, maxEnergy);
        }

        /// <summary>
        /// 에너지 회복 (float 오버로드)
        /// </summary>
        public void AddEnergy(float amount)
        {
            AddEnergy(Mathf.RoundToInt(amount));
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
        public void SetLevel(int newLevel, int newExp = 0)
        {
            level = newLevel;
            experience = newExp;

            // 레벨에 따른 스탯 재계산
            maxHealth = 100 + (level - 1) * healthPerLevel;
            maxEnergy = 100 + (level - 1) * energyPerLevel;
        }

        /// <summary>
        /// 경험치 설정 (저장/불러오기용)
        /// </summary>
        public void SetExperience(int exp)
        {
            experience = exp;
        }

        #endregion

        #region Gold

        /// <summary>
        /// 골드 획득
        /// </summary>
        public void AddGold(int amount)
        {
            if (amount <= 0) return;
            gold += amount;
            OnGoldChanged?.Invoke(gold);
            Debug.Log($"[PlayerStats] 골드 획득: +{amount} (현재: {gold})");
        }

        /// <summary>
        /// 골드 소비
        /// </summary>
        public bool SpendGold(int amount)
        {
            if (amount <= 0 || gold < amount) return false;
            gold -= amount;
            OnGoldChanged?.Invoke(gold);
            Debug.Log($"[PlayerStats] 골드 소비: -{amount} (현재: {gold})");
            return true;
        }

        /// <summary>
        /// 골드 설정 (저장/불러오기용)
        /// </summary>
        public void SetGold(int amount)
        {
            gold = Mathf.Max(0, amount);
            OnGoldChanged?.Invoke(gold);
        }

        #endregion

        #region Equipment Bonus

        /// <summary>
        /// 장비 보너스 설정
        /// </summary>
        public void SetEquipmentBonuses(int attack, int defense, int health, int energy)
        {
            equipmentAttackBonus = attack;
            equipmentDefenseBonus = defense;

            // 체력/에너지 보너스 적용
            int oldMaxHealth = maxHealth;
            int oldMaxEnergy = maxEnergy;

            maxHealth = 100 + (level - 1) * healthPerLevel + health;
            maxEnergy = 100 + (level - 1) * energyPerLevel + energy;

            // 보너스가 줄었을 때 현재 값 조정
            currentHealth = Mathf.Min(currentHealth, maxHealth);
            currentEnergy = Mathf.Min(currentEnergy, maxEnergy);

            if (oldMaxHealth != maxHealth)
                OnHealthChanged?.Invoke(currentHealth, maxHealth);
            if (oldMaxEnergy != maxEnergy)
                OnEnergyChanged?.Invoke(currentEnergy, maxEnergy);

            Debug.Log($"[PlayerStats] 장비 보너스 적용 - 공격: +{attack}, 방어: +{defense}, 체력: +{health}, 에너지: +{energy}");
        }

        /// <summary>
        /// 총 공격력 (기본 + 장비)
        /// </summary>
        public int GetTotalAttack(int baseAttack)
        {
            return baseAttack + equipmentAttackBonus;
        }

        /// <summary>
        /// 데미지 감소 계산
        /// </summary>
        public int ReduceDamage(int incomingDamage)
        {
            float reduction = equipmentDefenseBonus * 0.01f; // 1 방어력 = 1% 감소
            reduction = Mathf.Clamp(reduction, 0f, 0.75f);   // 최대 75% 감소
            return Mathf.Max(1, Mathf.RoundToInt(incomingDamage * (1f - reduction)));
        }

        #endregion

        private void Die()
        {
            Debug.Log("[PlayerStats] Player died!");
            OnPlayerDeath?.Invoke();
            OnDeath?.Invoke();
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
