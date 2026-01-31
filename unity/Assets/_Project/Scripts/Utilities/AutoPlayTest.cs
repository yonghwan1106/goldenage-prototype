using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;
using GoldenAge.Core;
using GoldenAge.Combat;
using GoldenAge.Player;

namespace GoldenAge.Utilities
{
    /// <summary>
    /// 자동 플레이 테스트 - AI가 게임을 끝까지 플레이
    /// </summary>
    public class AutoPlayTest : MonoBehaviour
    {
        [Header("자동 플레이 설정")]
        [SerializeField] private bool autoPlayEnabled = false;
        [SerializeField] private float actionInterval = 0.5f;
        [SerializeField] private float attackProbability = 0.7f;
        [SerializeField] private float skillProbability = 0.3f;
        [SerializeField] private float dodgeProbability = 0.2f;

        [Header("로그 설정")]
        [SerializeField] private bool verboseLog = true;
        [SerializeField] private float logInterval = 5f;

        [Header("테스트 결과")]
        [SerializeField] private float playTime = 0f;
        [SerializeField] private int totalDamageDealt = 0;
        [SerializeField] private int totalDamageTaken = 0;
        [SerializeField] private int enemiesKilled = 0;
        [SerializeField] private int skillsUsed = 0;
        [SerializeField] private int attacksPerformed = 0;

        // 캐시
        private Transform playerTransform;
        private PlayerStats playerStats;
        private PlayerCombat playerCombat;
        private PlayerMovement playerMovement;
        private CharacterController characterController;

        private float lastActionTime;
        private float lastLogTime;
        private bool isTestRunning = false;
        private bool playerDied = false;

        private List<string> testLog = new List<string>();

        private void Start()
        {
            if (autoPlayEnabled)
            {
                StartAutoPlay();
            }
        }

        /// <summary>
        /// 자동 플레이 시작
        /// </summary>
        public void StartAutoPlay()
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                LogTest("[AutoPlay] 플레이어를 찾을 수 없습니다!");
                return;
            }

            playerTransform = player.transform;
            playerStats = player.GetComponent<PlayerStats>();
            playerCombat = player.GetComponent<PlayerCombat>();
            playerMovement = player.GetComponent<PlayerMovement>();
            characterController = player.GetComponent<CharacterController>();

            if (playerStats == null)
            {
                LogTest("[AutoPlay] PlayerStats 컴포넌트가 없습니다!");
                return;
            }

            // 이벤트 구독
            playerStats.OnDeath += OnPlayerDeath;
            playerStats.OnDamaged += OnPlayerDamaged;

            isTestRunning = true;
            playerDied = false;
            playTime = 0f;
            totalDamageDealt = 0;
            totalDamageTaken = 0;
            enemiesKilled = 0;
            skillsUsed = 0;
            attacksPerformed = 0;

            LogTest("========================================");
            LogTest("[AutoPlay] 자동 플레이 테스트 시작!");
            LogTest($"[AutoPlay] 플레이어 체력: {playerStats.CurrentHealth}/{playerStats.MaxHealth}");
            LogTest($"[AutoPlay] 플레이어 에너지: {playerStats.CurrentEnergy}/{playerStats.MaxEnergy}");
            LogTest("========================================");

            StartCoroutine(AutoPlayCoroutine());
        }

        /// <summary>
        /// 자동 플레이 중지
        /// </summary>
        public void StopAutoPlay()
        {
            isTestRunning = false;
            PrintTestSummary();
        }

        private IEnumerator AutoPlayCoroutine()
        {
            while (isTestRunning && !playerDied)
            {
                playTime += Time.deltaTime;

                // 주기적 로그
                if (Time.time - lastLogTime > logInterval)
                {
                    LogStatus();
                    lastLogTime = Time.time;
                }

                // 행동 수행
                if (Time.time - lastActionTime > actionInterval)
                {
                    PerformAction();
                    lastActionTime = Time.time;
                }

                yield return null;
            }

            if (playerDied)
            {
                LogTest("========================================");
                LogTest("[AutoPlay] 플레이어 사망! 테스트 종료");
                PrintTestSummary();
                LogTest("========================================");
            }
        }

        private void PerformAction()
        {
            if (playerStats == null || !playerStats.IsAlive) return;

            // 가장 가까운 적 찾기
            var nearestEnemy = FindNearestEnemy();

            if (nearestEnemy != null)
            {
                float distance = Vector3.Distance(playerTransform.position, nearestEnemy.transform.position);

                // 적을 향해 이동
                MoveTowards(nearestEnemy.transform.position);

                // 공격 범위 내에 있으면 공격
                if (distance < 3f)
                {
                    float rand = Random.value;

                    if (rand < skillProbability && playerStats.CurrentEnergy >= 20)
                    {
                        // 스킬 사용
                        UseRandomSkill();
                    }
                    else if (rand < attackProbability)
                    {
                        // 기본 공격
                        PerformAttack(nearestEnemy);
                    }
                    else if (rand < dodgeProbability + attackProbability)
                    {
                        // 회피
                        PerformDodge();
                    }
                }
            }
            else
            {
                // 적이 없으면 랜덤하게 이동
                MoveRandom();
            }
        }

        private EnemyAI FindNearestEnemy()
        {
            var enemies = Object.FindObjectsByType<EnemyAI>(FindObjectsSortMode.None);
            EnemyAI nearest = null;
            float minDistance = float.MaxValue;

            foreach (var enemy in enemies)
            {
                if (enemy == null || !enemy.IsAlive) continue;

                float dist = Vector3.Distance(playerTransform.position, enemy.transform.position);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    nearest = enemy;
                }
            }

            return nearest;
        }

        private void MoveTowards(Vector3 target)
        {
            if (characterController == null) return;

            Vector3 direction = (target - playerTransform.position).normalized;
            direction.y = 0;

            // 플레이어 회전
            if (direction != Vector3.zero)
            {
                playerTransform.rotation = Quaternion.LookRotation(direction);
            }

            // 이동 (CharacterController 사용)
            Vector3 move = direction * 5f * Time.deltaTime;
            move.y = -9.81f * Time.deltaTime; // 중력
            characterController.Move(move);
        }

        private void MoveRandom()
        {
            if (characterController == null) return;

            Vector3 randomDir = new Vector3(
                Random.Range(-1f, 1f),
                0,
                Random.Range(-1f, 1f)
            ).normalized;

            Vector3 move = randomDir * 3f * Time.deltaTime;
            move.y = -9.81f * Time.deltaTime;
            characterController.Move(move);
        }

        private void PerformAttack(EnemyAI target)
        {
            if (target == null || !target.IsAlive) return;

            // 직접 데미지 적용 (애니메이션 없이 테스트)
            int damage = Random.Range(10, 25);
            target.TakeDamage(damage, DamageType.Physical);

            totalDamageDealt += damage;
            attacksPerformed++;

            if (verboseLog)
            {
                LogTest($"[AutoPlay] 공격! {target.name}에게 {damage} 데미지");
            }

            // 적이 죽었는지 확인
            if (!target.IsAlive)
            {
                enemiesKilled++;
                LogTest($"[AutoPlay] 적 처치! {target.name} (총 {enemiesKilled}마리)");
            }
        }

        private void UseRandomSkill()
        {
            int skillType = Random.Range(0, 3);
            string skillName = "";

            switch (skillType)
            {
                case 0:
                    skillName = "Tesla Shock";
                    break;
                case 1:
                    skillName = "Ether Wave";
                    break;
                case 2:
                    skillName = "Fusion Blast";
                    break;
            }

            // 에너지 소모
            playerStats.UseEnergy(20);
            skillsUsed++;

            // 범위 내 적들에게 데미지
            var enemies = Object.FindObjectsByType<EnemyAI>(FindObjectsSortMode.None);
            int damage = Random.Range(20, 40);

            foreach (var enemy in enemies)
            {
                if (enemy == null || !enemy.IsAlive) continue;

                float dist = Vector3.Distance(playerTransform.position, enemy.transform.position);
                if (dist < 5f)
                {
                    enemy.TakeDamage(damage, DamageType.Tesla);
                    totalDamageDealt += damage;

                    if (!enemy.IsAlive)
                    {
                        enemiesKilled++;
                        LogTest($"[AutoPlay] 스킬로 적 처치! {enemy.name}");
                    }
                }
            }

            if (verboseLog)
            {
                LogTest($"[AutoPlay] 스킬 사용: {skillName}");
            }
        }

        private void PerformDodge()
        {
            if (characterController == null) return;

            Vector3 dodgeDir = new Vector3(
                Random.Range(-1f, 1f),
                0,
                Random.Range(-1f, 1f)
            ).normalized;

            characterController.Move(dodgeDir * 3f);

            if (verboseLog)
            {
                LogTest("[AutoPlay] 회피!");
            }
        }

        private void OnPlayerDamaged(int damage)
        {
            totalDamageTaken += damage;

            if (verboseLog)
            {
                LogTest($"[AutoPlay] 피격! {damage} 데미지 (체력: {playerStats.CurrentHealth}/{playerStats.MaxHealth})");
            }
        }

        private void OnPlayerDeath()
        {
            playerDied = true;
            LogTest("[AutoPlay] 플레이어 사망!");
        }

        private void LogStatus()
        {
            if (playerStats == null) return;

            var enemies = Object.FindObjectsByType<EnemyAI>(FindObjectsSortMode.None);
            int aliveEnemies = 0;
            foreach (var e in enemies)
            {
                if (e != null && e.IsAlive) aliveEnemies++;
            }

            LogTest($"[AutoPlay] === 상태 보고 (플레이 시간: {playTime:F1}초) ===");
            LogTest($"  - 체력: {playerStats.CurrentHealth}/{playerStats.MaxHealth} ({playerStats.HealthPercent * 100:F0}%)");
            LogTest($"  - 에너지: {playerStats.CurrentEnergy}/{playerStats.MaxEnergy}");
            LogTest($"  - 생존 적 수: {aliveEnemies}");
            LogTest($"  - 처치한 적: {enemiesKilled}");
            LogTest($"  - 총 가한 데미지: {totalDamageDealt}");
            LogTest($"  - 총 받은 데미지: {totalDamageTaken}");
        }

        private void PrintTestSummary()
        {
            LogTest("");
            LogTest("╔══════════════════════════════════════╗");
            LogTest("║       자동 플레이 테스트 결과        ║");
            LogTest("╠══════════════════════════════════════╣");
            LogTest($"║  총 플레이 시간: {playTime:F1}초");
            LogTest($"║  처치한 적: {enemiesKilled}마리");
            LogTest($"║  총 공격 횟수: {attacksPerformed}회");
            LogTest($"║  스킬 사용 횟수: {skillsUsed}회");
            LogTest($"║  총 가한 데미지: {totalDamageDealt}");
            LogTest($"║  총 받은 데미지: {totalDamageTaken}");
            LogTest($"║  최종 체력: {(playerStats != null ? playerStats.CurrentHealth : 0)}/{(playerStats != null ? playerStats.MaxHealth : 0)}");
            LogTest("╚══════════════════════════════════════╝");

            // 분석
            LogTest("");
            LogTest("[분석]");

            if (playTime < 30f)
            {
                LogTest("⚠️ 플레이 시간이 너무 짧습니다 (30초 미만)");
                LogTest("  → 적의 수를 늘리거나 적의 체력을 높이세요");
            }

            if (enemiesKilled > 0 && totalDamageTaken == 0)
            {
                LogTest("⚠️ 플레이어가 데미지를 전혀 받지 않았습니다");
                LogTest("  → 적의 공격력을 높이거나 공격 속도를 빠르게 하세요");
            }

            if (totalDamageDealt > totalDamageTaken * 10)
            {
                LogTest("⚠️ 플레이어가 너무 강합니다 (데미지 비율 10:1 이상)");
                LogTest("  → 밸런스 조정이 필요합니다");
            }

            var remainingEnemies = Object.FindObjectsByType<EnemyAI>(FindObjectsSortMode.None);
            int alive = 0;
            foreach (var e in remainingEnemies)
            {
                if (e != null && e.IsAlive) alive++;
            }

            if (alive == 0 && !playerDied)
            {
                LogTest("⚠️ 모든 적을 처치했지만 플레이어가 살아있습니다");
                LogTest("  → 더 많은 적이나 웨이브 시스템이 필요합니다");
            }
        }

        private void LogTest(string message)
        {
            testLog.Add($"[{Time.time:F1}] {message}");
            Debug.Log(message);
        }

        private void OnDestroy()
        {
            if (playerStats != null)
            {
                playerStats.OnDeath -= OnPlayerDeath;
                playerStats.OnDamaged -= OnPlayerDamaged;
            }
        }

        // 에디터에서 테스트 시작/중지
        [ContextMenu("Start Auto Play Test")]
        public void EditorStartTest()
        {
            StartAutoPlay();
        }

        [ContextMenu("Stop Auto Play Test")]
        public void EditorStopTest()
        {
            StopAutoPlay();
        }
    }
}
