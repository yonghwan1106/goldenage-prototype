using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;
using GoldenAge.Core;
using GoldenAge.Player;

namespace GoldenAge.Combat
{
    /// <summary>
    /// 플레이어 전투 시스템
    /// </summary>
    public class PlayerCombat : MonoBehaviour
    {
        [Header("공격 데이터")]
        [SerializeField] private AttackData meleeAttack;
        [SerializeField] private AttackData rangedAttack;
        [SerializeField] private AttackData teslaShock;      // 테슬라 충격기
        [SerializeField] private AttackData etherWave;       // 에테르 파동
        [SerializeField] private AttackData fusionBlast;     // 융합 콤보

        [Header("참조")]
        [SerializeField] private Transform attackPoint;
        [SerializeField] private LayerMask enemyLayer;

        private PlayerInputActions inputActions;
        private PlayerStats playerStats;
        private PlayerAnimator playerAnimator;
        private Dictionary<AttackData, float> lastUseTimes = new Dictionary<AttackData, float>();
        private bool canAttack = true;

        // 융합 콤보 추적
        private bool teslaHitRecently = false;
        private bool etherHitRecently = false;
        private float fusionWindowTimer = 0f;
        private const float FUSION_WINDOW = 3f;

        // Events
        public event System.Action<AttackData> OnAttackPerformed;
        public event System.Action OnFusionComboTriggered;

        private void Awake()
        {
            inputActions = new PlayerInputActions();
            playerStats = GetComponent<PlayerStats>();
            playerAnimator = GetComponent<PlayerAnimator>();

            if (attackPoint == null)
            {
                attackPoint = transform;
            }

            InitializeCooldowns();
        }

        private void OnEnable()
        {
            inputActions.Player.Enable();
            inputActions.Player.Attack.performed += OnAttack;
            inputActions.Player.Skill1.performed += OnTeslaShock;
            inputActions.Player.Skill2.performed += OnEtherWave;
        }

        private void OnDisable()
        {
            inputActions.Player.Attack.performed -= OnAttack;
            inputActions.Player.Skill1.performed -= OnTeslaShock;
            inputActions.Player.Skill2.performed -= OnEtherWave;
            inputActions.Player.Disable();
        }

        private void Update()
        {
            UpdateFusionWindow();
        }

        private void InitializeCooldowns()
        {
            float startTime = -100f; // 시작 시 즉시 사용 가능
            if (meleeAttack != null) lastUseTimes[meleeAttack] = startTime;
            if (rangedAttack != null) lastUseTimes[rangedAttack] = startTime;
            if (teslaShock != null) lastUseTimes[teslaShock] = startTime;
            if (etherWave != null) lastUseTimes[etherWave] = startTime;
            if (fusionBlast != null) lastUseTimes[fusionBlast] = startTime;
        }

        private void UpdateFusionWindow()
        {
            if (teslaHitRecently || etherHitRecently)
            {
                fusionWindowTimer -= Time.deltaTime;
                if (fusionWindowTimer <= 0)
                {
                    teslaHitRecently = false;
                    etherHitRecently = false;
                }
            }
        }

        #region Input Handlers

        private void OnAttack(InputAction.CallbackContext ctx)
        {
            if (!CanPerformAttack()) return;

            if (meleeAttack != null && IsReady(meleeAttack))
            {
                StartCoroutine(ExecuteAttack(meleeAttack));
            }
        }

        private void OnTeslaShock(InputAction.CallbackContext ctx)
        {
            if (!CanPerformAttack()) return;

            if (teslaShock != null && IsReady(teslaShock))
            {
                if (playerStats == null || playerStats.UseEnergy(teslaShock.energyCost))
                {
                    StartCoroutine(ExecuteAttack(teslaShock));
                }
                else
                {
                    Debug.Log("[PlayerCombat] Not enough energy for Tesla Shock");
                }
            }
        }

        private void OnEtherWave(InputAction.CallbackContext ctx)
        {
            if (!CanPerformAttack()) return;

            if (etherWave != null && IsReady(etherWave))
            {
                if (playerStats == null || playerStats.UseEnergy(etherWave.energyCost))
                {
                    StartCoroutine(ExecuteAttack(etherWave));
                }
                else
                {
                    Debug.Log("[PlayerCombat] Not enough energy for Ether Wave");
                }
            }
        }

        #endregion

        #region Attack Execution

        private IEnumerator ExecuteAttack(AttackData attack)
        {
            canAttack = false;
            lastUseTimes[attack] = Time.time;

            // 시전 시간
            if (attack.castTime > 0)
            {
                // 시전 VFX
                if (attack.castVFX != null)
                {
                    Instantiate(attack.castVFX, attackPoint.position, attackPoint.rotation);
                }
                yield return new WaitForSeconds(attack.castTime);
            }

            // 애니메이션 트리거
            if (!string.IsNullOrEmpty(attack.animationTrigger))
            {
                playerAnimator?.TriggerAttack();
            }

            // 사운드
            if (attack.castSFX != null)
            {
                AudioManager.Instance?.PlaySFX(attack.castSFX);
            }

            // 판정 실행
            List<IDamageable> hitTargets = PerformHitDetection(attack);

            // 데미지 적용
            int playerLevel = playerStats != null ? playerStats.Level : 1;

            foreach (var target in hitTargets)
            {
                int finalDamage = CombatManager.Instance?.CalculateDamage(
                    attack.baseDamage, playerLevel, 0) ?? attack.baseDamage;

                // 크리티컬 체크
                if (CombatManager.Instance?.RollCritical() == true)
                {
                    finalDamage = CombatManager.Instance.ApplyCritical(finalDamage);
                    Debug.Log("[PlayerCombat] Critical Hit!");
                }

                target.TakeDamage(finalDamage, attack.damageType);

                // 상태이상 적용
                if (attack.applyEffects != null)
                {
                    foreach (var effect in attack.applyEffects)
                    {
                        if (Random.value <= attack.effectChance)
                        {
                            target.ApplyEffect(effect);
                        }
                    }
                }

                // 융합 콤보 체크
                CheckFusionCombo(attack, target);
            }

            // 이펙트 스폰
            SpawnHitEffects(attack, hitTargets);

            OnAttackPerformed?.Invoke(attack);

            yield return new WaitForSeconds(attack.animationDuration);
            canAttack = true;
        }

        private List<IDamageable> PerformHitDetection(AttackData attack)
        {
            List<IDamageable> targets = new List<IDamageable>();

            if (attack.isAOE)
            {
                // AOE 판정
                Collider[] hits = Physics.OverlapSphere(
                    attackPoint.position,
                    attack.aoeRadius,
                    enemyLayer
                );

                foreach (var hit in hits)
                {
                    if (hit.TryGetComponent<IDamageable>(out var damageable))
                    {
                        // Cone 형태면 각도 체크
                        if (attack.aoeShape == AOEShape.Cone)
                        {
                            Vector3 dirToTarget = (hit.transform.position - transform.position).normalized;
                            float angle = Vector3.Angle(transform.forward, dirToTarget);
                            if (angle > attack.coneAngle / 2f)
                                continue;
                        }

                        targets.Add(damageable);
                    }
                }
            }
            else
            {
                // 단일 대상 판정
                if (Physics.Raycast(
                    attackPoint.position,
                    transform.forward,
                    out RaycastHit hit,
                    attack.range,
                    enemyLayer))
                {
                    if (hit.collider.TryGetComponent<IDamageable>(out var damageable))
                    {
                        targets.Add(damageable);
                    }
                }
            }

            return targets;
        }

        private void SpawnHitEffects(AttackData attack, List<IDamageable> hitTargets)
        {
            foreach (var target in hitTargets)
            {
                MonoBehaviour targetMono = target as MonoBehaviour;
                if (targetMono == null) continue;

                Vector3 hitPosition = targetMono.transform.position + Vector3.up;

                // VFX 스폰
                if (attack.hitVFX != null)
                {
                    GameObject vfx = Instantiate(attack.hitVFX, hitPosition, Quaternion.identity);
                    Destroy(vfx, 2f);
                }

                // SFX 재생
                if (attack.hitSFX != null)
                {
                    AudioManager.Instance?.PlaySFXAtPosition(attack.hitSFX, hitPosition);
                }
            }
        }

        #endregion

        #region Fusion Combo

        private void CheckFusionCombo(AttackData attack, IDamageable target)
        {
            if (attack == teslaShock)
            {
                teslaHitRecently = true;
                fusionWindowTimer = FUSION_WINDOW;
            }
            else if (attack == etherWave)
            {
                etherHitRecently = true;
                fusionWindowTimer = FUSION_WINDOW;
            }

            // 융합 조건 충족 시 자동 발동
            if (teslaHitRecently && etherHitRecently && fusionBlast != null && IsReady(fusionBlast))
            {
                StartCoroutine(ExecuteFusionBlast());
            }
        }

        private IEnumerator ExecuteFusionBlast()
        {
            teslaHitRecently = false;
            etherHitRecently = false;
            lastUseTimes[fusionBlast] = Time.time;

            OnFusionComboTriggered?.Invoke();
            Debug.Log("[PlayerCombat] Fusion Blast - Dimensional Strike!");

            // VFX
            if (fusionBlast.castVFX != null)
            {
                Instantiate(fusionBlast.castVFX, transform.position, Quaternion.identity);
            }

            // 범위 내 모든 적에게 피해
            Collider[] hits = Physics.OverlapSphere(
                transform.position,
                fusionBlast.aoeRadius,
                enemyLayer
            );

            foreach (var hit in hits)
            {
                if (hit.TryGetComponent<IDamageable>(out var damageable))
                {
                    damageable.TakeDamage(fusionBlast.baseDamage, DamageType.Fusion);

                    if (fusionBlast.applyEffects != null)
                    {
                        foreach (var effect in fusionBlast.applyEffects)
                        {
                            damageable.ApplyEffect(effect);
                        }
                    }
                }
            }

            yield return null;
        }

        #endregion

        #region Helpers

        private bool CanPerformAttack()
        {
            if (!canAttack) return false;

            if (GameManager.Instance != null && !GameManager.Instance.CanPlayerInput())
                return false;

            return true;
        }

        private bool IsReady(AttackData attack)
        {
            if (attack == null) return false;

            if (!lastUseTimes.TryGetValue(attack, out float lastUse))
                return true;

            return attack.IsReady(lastUse);
        }

        public float GetCooldownPercent(AttackData attack)
        {
            if (attack == null) return 0f;

            if (!lastUseTimes.TryGetValue(attack, out float lastUse))
                return 0f;

            return attack.GetCooldownProgress(lastUse);
        }

        public bool IsSkillReady(int skillIndex)
        {
            return skillIndex switch
            {
                0 => teslaShock != null && IsReady(teslaShock),
                1 => etherWave != null && IsReady(etherWave),
                _ => false
            };
        }

        #endregion

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (attackPoint == null) return;

            // 기본 공격 범위
            if (meleeAttack != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawRay(attackPoint.position, transform.forward * meleeAttack.range);
            }

            // 테슬라 충격 범위
            if (teslaShock != null && teslaShock.isAOE)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(attackPoint.position, teslaShock.aoeRadius);
            }

            // 융합 폭발 범위
            if (fusionBlast != null && fusionBlast.isAOE)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(transform.position, fusionBlast.aoeRadius);
            }
        }
#endif
    }
}
