using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using GoldenAge.Core;

namespace GoldenAge.Combat
{
    /// <summary>
    /// 적 AI 상태
    /// </summary>
    public enum EnemyState
    {
        Idle,
        Patrol,
        Chase,
        Attack,
        Dead
    }

    /// <summary>
    /// 적 AI 및 전투 로직
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public class EnemyAI : MonoBehaviour, IDamageable
    {
        [Header("캐릭터 데이터")]
        [SerializeField] private CharacterData characterData;
        [SerializeField] private string enemyID = "enemy";

        [Header("AI 설정")]
        [SerializeField] private float detectionRange = 10f;
        [SerializeField] private float attackRange = 2f;
        [SerializeField] private float patrolSpeed = 2f;
        [SerializeField] private float chaseSpeed = 4f;
        [SerializeField] private float attackCooldown = 2f;

        [Header("순찰 (선택)")]
        [SerializeField] private Transform[] patrolPoints;
        [SerializeField] private float patrolWaitTime = 2f;

        private int currentHealth;
        private Transform player;
        private NavMeshAgent agent;
        private Animator animator;
        private EnemyState currentState = EnemyState.Idle;

        private float lastAttackTime;
        private int currentPatrolIndex;
        private float patrolWaitTimer;

        // 상태이상
        private List<ActiveEffect> activeEffects = new List<ActiveEffect>();
        private bool isStunned = false;
        private float baseSpeed;

        // Properties
        public string EnemyID => enemyID;
        public int ExpReward => characterData != null ? characterData.expReward : 25;
        public int CurrentHealth => currentHealth;
        public int MaxHealth => characterData != null ? characterData.maxHealth : 100;
        public bool IsAlive => currentHealth > 0;
        public EnemyState State => currentState;

        // Events
        public event System.Action<int, int> OnHealthChanged;
        public event System.Action OnDeath;

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            animator = GetComponent<Animator>();

            if (characterData != null)
            {
                currentHealth = characterData.maxHealth;
                baseSpeed = characterData.moveSpeed;
            }
            else
            {
                currentHealth = 100;
                baseSpeed = patrolSpeed;
            }
        }

        private void Start()
        {
            player = GameObject.FindGameObjectWithTag("Player")?.transform;

            if (patrolPoints != null && patrolPoints.Length > 0)
            {
                currentState = EnemyState.Patrol;
            }
        }

        private void Update()
        {
            if (currentState == EnemyState.Dead) return;

            UpdateEffects();

            if (isStunned) return;

            switch (currentState)
            {
                case EnemyState.Idle:
                    IdleBehavior();
                    break;
                case EnemyState.Patrol:
                    PatrolBehavior();
                    break;
                case EnemyState.Chase:
                    ChaseBehavior();
                    break;
                case EnemyState.Attack:
                    AttackBehavior();
                    break;
            }
        }

        #region AI Behaviors

        private void IdleBehavior()
        {
            CheckForPlayer();
        }

        private void PatrolBehavior()
        {
            CheckForPlayer();

            if (patrolPoints == null || patrolPoints.Length == 0) return;

            if (!agent.pathPending && agent.remainingDistance < 0.5f)
            {
                patrolWaitTimer += Time.deltaTime;
                if (patrolWaitTimer >= patrolWaitTime)
                {
                    patrolWaitTimer = 0f;
                    currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
                    agent.SetDestination(patrolPoints[currentPatrolIndex].position);
                }
            }

            agent.speed = patrolSpeed;
        }

        private void ChaseBehavior()
        {
            if (player == null) return;

            float distance = Vector3.Distance(transform.position, player.position);

            if (distance <= attackRange)
            {
                ChangeState(EnemyState.Attack);
                agent.isStopped = true;
            }
            else if (distance > detectionRange * 1.5f)
            {
                ChangeState(EnemyState.Idle);
                agent.isStopped = true;
            }
            else
            {
                agent.isStopped = false;
                agent.speed = chaseSpeed;
                agent.SetDestination(player.position);
            }
        }

        private void AttackBehavior()
        {
            if (player == null) return;

            float distance = Vector3.Distance(transform.position, player.position);

            // 범위 벗어나면 추적
            if (distance > attackRange * 1.2f)
            {
                ChangeState(EnemyState.Chase);
                return;
            }

            // 플레이어 바라보기
            Vector3 lookDir = (player.position - transform.position).normalized;
            lookDir.y = 0;
            transform.rotation = Quaternion.LookRotation(lookDir);

            // 공격 쿨다운 체크
            if (Time.time >= lastAttackTime + attackCooldown)
            {
                PerformAttack();
                lastAttackTime = Time.time;
            }
        }

        private void CheckForPlayer()
        {
            if (player == null) return;

            float distance = Vector3.Distance(transform.position, player.position);
            if (distance <= detectionRange)
            {
                ChangeState(EnemyState.Chase);
                CombatManager.Instance?.RegisterEnemy(this);
            }
        }

        private void ChangeState(EnemyState newState)
        {
            if (currentState == newState) return;
            currentState = newState;
            Debug.Log($"[EnemyAI] {name} state: {newState}");
        }

        #endregion

        #region Combat

        private void PerformAttack()
        {
            if (characterData?.basicAttack != null)
            {
                // 애니메이션 트리거
                animator?.SetTrigger("Attack");

                // 플레이어에게 데미지 (애니메이션 이벤트로 처리하는 것이 좋음)
                var playerStats = player?.GetComponent<Player.PlayerStats>();
                if (playerStats != null)
                {
                    int damage = characterData.attackPower;
                    playerStats.TakeDamage(damage);
                    Debug.Log($"[EnemyAI] {name} attacked for {damage} damage");
                }
            }
        }

        #endregion

        #region IDamageable

        public void TakeDamage(int damage, DamageType damageType)
        {
            if (!IsAlive) return;

            // 방어력 적용
            int defense = characterData != null ? characterData.defense : 0;
            int finalDamage = Mathf.Max(1, damage - defense);
            currentHealth -= finalDamage;

            OnHealthChanged?.Invoke(currentHealth, MaxHealth);
            Debug.Log($"[EnemyAI] {name} took {finalDamage} damage. HP: {currentHealth}/{MaxHealth}");

            // 피격 애니메이션
            animator?.SetTrigger("Hit");

            // 전투 상태로 전환
            if (currentState == EnemyState.Idle || currentState == EnemyState.Patrol)
            {
                ChangeState(EnemyState.Chase);
                CombatManager.Instance?.RegisterEnemy(this);
            }

            if (currentHealth <= 0)
            {
                Die();
            }
        }

        public void ApplyEffect(StatusEffect effect)
        {
            if (effect == null || !IsAlive) return;

            ActiveEffect active = new ActiveEffect(effect);
            activeEffects.Add(active);

            // 즉시 효과 적용
            if (effect.isStunned)
                isStunned = true;
            if (effect.slowPercent > 0)
                agent.speed *= (1f - effect.slowPercent);

            // VFX 스폰
            if (effect.effectVFX != null)
            {
                active.vfxInstance = Instantiate(effect.effectVFX, transform);
            }

            Debug.Log($"[EnemyAI] {name} received effect: {effect.effectName}");
        }

        private void UpdateEffects()
        {
            for (int i = activeEffects.Count - 1; i >= 0; i--)
            {
                var active = activeEffects[i];
                active.Update(Time.deltaTime);

                // 도트 데미지
                if (active.effect.damagePerTick > 0 && active.ShouldTick())
                {
                    currentHealth -= active.effect.damagePerTick;
                    OnHealthChanged?.Invoke(currentHealth, MaxHealth);

                    if (currentHealth <= 0)
                    {
                        Die();
                        return;
                    }
                }

                // 효과 종료
                if (active.IsExpired)
                {
                    RemoveEffect(active);
                    activeEffects.RemoveAt(i);
                }
            }
        }

        private void RemoveEffect(ActiveEffect active)
        {
            if (active.effect.isStunned)
                isStunned = false;
            if (active.effect.slowPercent > 0)
                agent.speed = baseSpeed;

            if (active.vfxInstance != null)
                Destroy(active.vfxInstance);
        }

        #endregion

        private void Die()
        {
            currentState = EnemyState.Dead;
            agent.isStopped = true;

            // 애니메이션
            animator?.SetTrigger("Death");

            // 이벤트
            OnDeath?.Invoke();
            CombatManager.Instance?.UnregisterEnemy(this);

            // 사운드
            if (characterData?.deathSound != null)
            {
                AudioManager.Instance?.PlaySFX(characterData.deathSound);
            }

            // 콜라이더 비활성화
            var colliders = GetComponents<Collider>();
            foreach (var col in colliders)
            {
                col.enabled = false;
            }

            // 일정 시간 후 제거
            Destroy(gameObject, 3f);
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            // 감지 범위
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRange);

            // 공격 범위
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);
        }
#endif
    }
}
