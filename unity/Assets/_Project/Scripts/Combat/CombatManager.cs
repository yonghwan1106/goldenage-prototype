using UnityEngine;
using System.Collections.Generic;
using GoldenAge.Core;

namespace GoldenAge.Combat
{
    /// <summary>
    /// 전투 상태 및 적 관리를 담당하는 매니저
    /// </summary>
    public class CombatManager : Singleton<CombatManager>
    {
        [Header("전투 설정")]
        [SerializeField] private float combatExitDelay = 5f;

        private bool isInCombat;
        private float lastCombatTime;
        private List<EnemyAI> activeEnemies = new List<EnemyAI>();

        // Properties
        public bool IsInCombat => isInCombat;
        public int ActiveEnemyCount => activeEnemies.Count;

        // Events
        public event System.Action OnCombatStart;
        public event System.Action OnCombatEnd;
        public event System.Action<EnemyAI> OnEnemyKilled;

        private void Update()
        {
            // 전투 종료 체크
            if (isInCombat && activeEnemies.Count == 0)
            {
                if (Time.time - lastCombatTime > combatExitDelay)
                {
                    EndCombat();
                }
            }
        }

        /// <summary>
        /// 전투 시작
        /// </summary>
        public void EnterCombat()
        {
            if (isInCombat) return;

            isInCombat = true;
            lastCombatTime = Time.time;
            GameManager.Instance?.ChangeState(GameState.Combat);
            OnCombatStart?.Invoke();

            AudioManager.Instance?.PlayBGM("Combat");
            Debug.Log("[CombatManager] Combat started!");
        }

        /// <summary>
        /// 전투 종료
        /// </summary>
        public void EndCombat()
        {
            if (!isInCombat) return;

            isInCombat = false;
            GameManager.Instance?.ChangeState(GameState.Exploration);
            OnCombatEnd?.Invoke();

            AudioManager.Instance?.PlayBGM("Exploration");
            Debug.Log("[CombatManager] Combat ended!");
        }

        /// <summary>
        /// 적 등록
        /// </summary>
        public void RegisterEnemy(EnemyAI enemy)
        {
            if (enemy == null) return;

            if (!activeEnemies.Contains(enemy))
            {
                activeEnemies.Add(enemy);
                EnterCombat();
                Debug.Log($"[CombatManager] Enemy registered: {enemy.name}. Total: {activeEnemies.Count}");
            }
        }

        /// <summary>
        /// 적 제거 (사망 시 호출)
        /// </summary>
        public void UnregisterEnemy(EnemyAI enemy)
        {
            if (enemy == null) return;

            if (activeEnemies.Remove(enemy))
            {
                lastCombatTime = Time.time;
                OnEnemyKilled?.Invoke(enemy);
                Debug.Log($"[CombatManager] Enemy killed: {enemy.name}. Remaining: {activeEnemies.Count}");
            }
        }

        /// <summary>
        /// 데미지 계산
        /// </summary>
        public int CalculateDamage(int baseDamage, int attackerLevel, int defenderDefense)
        {
            // 기본 공식: 기본 데미지 * (1 + 레벨 보너스) - 방어력
            float levelBonus = 1f + (attackerLevel - 1) * 0.1f;
            int rawDamage = Mathf.RoundToInt(baseDamage * levelBonus);
            int finalDamage = Mathf.Max(1, rawDamage - defenderDefense);

            return finalDamage;
        }

        /// <summary>
        /// 크리티컬 히트 판정
        /// </summary>
        public bool RollCritical(float critChance = 0.1f)
        {
            return Random.value < critChance;
        }

        /// <summary>
        /// 크리티컬 데미지 적용
        /// </summary>
        public int ApplyCritical(int damage, float critMultiplier = 1.5f)
        {
            return Mathf.RoundToInt(damage * critMultiplier);
        }

        /// <summary>
        /// 모든 적 제거 (디버그/치트용)
        /// </summary>
        [ContextMenu("Debug: Kill All Enemies")]
        public void KillAllEnemies()
        {
            var enemies = new List<EnemyAI>(activeEnemies);
            foreach (var enemy in enemies)
            {
                if (enemy != null)
                {
                    enemy.TakeDamage(99999, DamageType.Physical);
                }
            }
        }
    }
}
