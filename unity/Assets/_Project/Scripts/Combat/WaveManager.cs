using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using GoldenAge.Core;
using GoldenAge.Player;

namespace GoldenAge.Combat
{
    /// <summary>
    /// 웨이브 시스템 매니저 - 게임 전체의 웨이브를 관리
    /// </summary>
    public class WaveManager : Singleton<WaveManager>
    {
        [Header("웨이브 설정")]
        [SerializeField] private List<WaveConfig> waveConfigs = new List<WaveConfig>();
        [SerializeField] private float timeBetweenWaves = 5f;
        [SerializeField] private bool autoStartFirstWave = true;
        [SerializeField] private float initialDelay = 3f;

        [Header("스폰 설정")]
        [SerializeField] private GameObject[] enemyPrefabs;
        [SerializeField] private Transform[] spawnPoints;
        [SerializeField] private float spawnRadius = 3f;
        [SerializeField] private int maxEnemiesAlive = 15;

        [Header("난이도 스케일링")]
        [SerializeField] private float healthScalePerWave = 0.1f;  // 웨이브당 체력 10% 증가
        [SerializeField] private float damageScalePerWave = 0.05f; // 웨이브당 데미지 5% 증가
        [SerializeField] private int extraEnemiesPerWave = 2;      // 웨이브당 추가 적

        [Header("무한 모드")]
        [SerializeField] private bool infiniteWaves = true;
        [SerializeField] private int baseEnemyCount = 5;

        // 상태
        private int currentWave = 0;
        private int enemiesAlive = 0;
        private int enemiesKilledThisWave = 0;
        private int totalEnemiesKilled = 0;
        private bool isWaveActive = false;
        private bool isPaused = false;

        // 활성 적 추적
        private List<EnemyAI> activeEnemies = new List<EnemyAI>();

        // 이벤트
        public event Action<int> OnWaveStart;           // 웨이브 번호
        public event Action<int, int> OnWaveComplete;   // 웨이브 번호, 처치 수
        public event Action<int> OnEnemyKilled;         // 총 처치 수
        public event Action<int, int> OnEnemyCountChanged; // 현재, 최대
        public event Action OnAllWavesComplete;
        public event Action OnGameOver;

        // Properties
        public int CurrentWave => currentWave;
        public int EnemiesAlive => enemiesAlive;
        public int TotalEnemiesKilled => totalEnemiesKilled;
        public bool IsWaveActive => isWaveActive;
        public float WaveProgress => waveConfigs.Count > 0 && currentWave > 0
            ? (float)enemiesKilledThisWave / GetCurrentWaveEnemyCount()
            : 0f;

        protected override void Awake()
        {
            base.Awake();

            // 기본 웨이브 설정이 없으면 생성
            if (waveConfigs.Count == 0)
            {
                GenerateDefaultWaves();
            }
        }

        private void Start()
        {
            // 플레이어 사망 이벤트 구독
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                var stats = player.GetComponent<PlayerStats>();
                if (stats != null)
                {
                    stats.OnDeath += HandlePlayerDeath;
                }
            }

            if (autoStartFirstWave)
            {
                StartCoroutine(StartWaveSystemDelayed());
            }
        }

        private IEnumerator StartWaveSystemDelayed()
        {
            yield return new WaitForSeconds(initialDelay);
            StartNextWave();
        }

        /// <summary>
        /// 기본 웨이브 생성 (10개 웨이브)
        /// </summary>
        private void GenerateDefaultWaves()
        {
            waveConfigs.Clear();

            for (int i = 1; i <= 10; i++)
            {
                var wave = new WaveConfig
                {
                    waveName = $"Wave {i}",
                    enemyCount = baseEnemyCount + (i - 1) * extraEnemiesPerWave,
                    spawnInterval = Mathf.Max(0.5f, 2f - i * 0.1f),
                    expBonus = 50 * i,
                    goldBonus = 20 * i
                };

                // 5웨이브마다 보스 추가
                if (i % 5 == 0)
                {
                    wave.waveName = $"Boss Wave {i}";
                    wave.isBossWave = true;
                    wave.expBonus *= 3;
                    wave.goldBonus *= 3;
                }

                waveConfigs.Add(wave);
            }

            Debug.Log($"[WaveManager] {waveConfigs.Count}개의 기본 웨이브 생성됨");
        }

        /// <summary>
        /// 다음 웨이브 시작
        /// </summary>
        public void StartNextWave()
        {
            if (isWaveActive) return;

            currentWave++;

            // 무한 모드에서 웨이브 자동 생성
            if (infiniteWaves && currentWave > waveConfigs.Count)
            {
                GenerateInfiniteWave(currentWave);
            }

            if (currentWave <= waveConfigs.Count || infiniteWaves)
            {
                StartCoroutine(SpawnWaveCoroutine());
            }
            else
            {
                Debug.Log("[WaveManager] 모든 웨이브 완료!");
                OnAllWavesComplete?.Invoke();
            }
        }

        /// <summary>
        /// 무한 모드용 웨이브 생성
        /// </summary>
        private void GenerateInfiniteWave(int waveNum)
        {
            var wave = new WaveConfig
            {
                waveName = $"Endless Wave {waveNum}",
                enemyCount = baseEnemyCount + (waveNum - 1) * extraEnemiesPerWave,
                spawnInterval = Mathf.Max(0.3f, 2f - waveNum * 0.05f),
                expBonus = 50 * waveNum,
                goldBonus = 20 * waveNum,
                isBossWave = waveNum % 5 == 0
            };

            waveConfigs.Add(wave);
        }

        private IEnumerator SpawnWaveCoroutine()
        {
            isWaveActive = true;
            enemiesKilledThisWave = 0;

            WaveConfig config = GetCurrentWaveConfig();
            int enemyCount = GetCurrentWaveEnemyCount();

            Debug.Log($"[WaveManager] === 웨이브 {currentWave} 시작! === 적 {enemyCount}마리");
            OnWaveStart?.Invoke(currentWave);

            // UI 알림
            NotificationSystem.Instance?.Show(
                $"웨이브 {currentWave}",
                config.isBossWave ? "보스 등장!" : $"적 {enemyCount}마리 출현!",
                NotificationType.Info
            );

            // 적 스폰
            for (int i = 0; i < enemyCount; i++)
            {
                // 최대 수 제한 대기
                while (enemiesAlive >= maxEnemiesAlive)
                {
                    yield return new WaitForSeconds(0.5f);
                }

                SpawnEnemy(config);
                yield return new WaitForSeconds(config.spawnInterval);
            }

            // 모든 적이 죽을 때까지 대기
            yield return new WaitUntil(() => enemiesAlive == 0);

            // 웨이브 완료 처리
            WaveCompleted();
        }

        private void SpawnEnemy(WaveConfig config)
        {
            if (enemyPrefabs == null || enemyPrefabs.Length == 0)
            {
                Debug.LogWarning("[WaveManager] 적 프리팹이 설정되지 않음!");
                return;
            }

            // 프리팹 선택
            GameObject prefab = config.specificEnemyPrefab != null
                ? config.specificEnemyPrefab
                : enemyPrefabs[UnityEngine.Random.Range(0, enemyPrefabs.Length)];

            // 스폰 위치
            Vector3 spawnPos = GetSpawnPosition();

            // 스폰
            GameObject enemy = Instantiate(prefab, spawnPos, Quaternion.identity);

            // 난이도 스케일링 적용
            ApplyDifficultyScaling(enemy);

            // EnemyAI 연결
            EnemyAI enemyAI = enemy.GetComponent<EnemyAI>();
            if (enemyAI != null)
            {
                activeEnemies.Add(enemyAI);
                enemyAI.OnDeath += () => HandleEnemyDeath(enemyAI);
            }

            enemiesAlive++;
            OnEnemyCountChanged?.Invoke(enemiesAlive, GetCurrentWaveEnemyCount());
        }

        private Vector3 GetSpawnPosition()
        {
            Vector3 basePos;

            if (spawnPoints != null && spawnPoints.Length > 0)
            {
                Transform point = spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Length)];
                basePos = point.position;
            }
            else
            {
                // 플레이어 주변 (화면 밖)
                var player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    float angle = UnityEngine.Random.Range(0f, 360f) * Mathf.Deg2Rad;
                    float distance = 15f + UnityEngine.Random.Range(0f, 5f);
                    basePos = player.transform.position + new Vector3(
                        Mathf.Cos(angle) * distance,
                        0,
                        Mathf.Sin(angle) * distance
                    );
                }
                else
                {
                    basePos = Vector3.zero;
                }
            }

            // 랜덤 오프셋
            Vector2 offset = UnityEngine.Random.insideUnitCircle * spawnRadius;
            return basePos + new Vector3(offset.x, 0, offset.y);
        }

        private void ApplyDifficultyScaling(GameObject enemy)
        {
            // 체력 스케일링은 CharacterData에서 처리하거나
            // 여기서 직접 수정할 수 있음
            float healthMultiplier = 1f + (currentWave - 1) * healthScalePerWave;
            float damageMultiplier = 1f + (currentWave - 1) * damageScalePerWave;

            // EnemyAI에 스케일 적용 (선택적)
            // enemy.GetComponent<EnemyAI>()?.ApplyScaling(healthMultiplier, damageMultiplier);
        }

        private void HandleEnemyDeath(EnemyAI enemy)
        {
            enemiesAlive--;
            enemiesKilledThisWave++;
            totalEnemiesKilled++;

            activeEnemies.Remove(enemy);

            OnEnemyKilled?.Invoke(totalEnemiesKilled);
            OnEnemyCountChanged?.Invoke(enemiesAlive, GetCurrentWaveEnemyCount());

            Debug.Log($"[WaveManager] 적 처치! ({enemiesKilledThisWave}/{GetCurrentWaveEnemyCount()}) 총 처치: {totalEnemiesKilled}");
        }

        private void WaveCompleted()
        {
            isWaveActive = false;

            WaveConfig config = GetCurrentWaveConfig();

            // 보상 지급
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                var stats = player.GetComponent<PlayerStats>();
                if (stats != null)
                {
                    stats.AddExperience(config.expBonus);
                    stats.AddGold(config.goldBonus);
                }
            }

            Debug.Log($"[WaveManager] === 웨이브 {currentWave} 완료! === EXP: +{config.expBonus}, 골드: +{config.goldBonus}");
            OnWaveComplete?.Invoke(currentWave, enemiesKilledThisWave);

            // UI 알림
            NotificationSystem.Instance?.Show(
                $"웨이브 {currentWave} 클리어!",
                $"보상: EXP +{config.expBonus}, 골드 +{config.goldBonus}",
                NotificationType.Success
            );

            // 다음 웨이브 자동 시작
            if (infiniteWaves || currentWave < waveConfigs.Count)
            {
                StartCoroutine(StartNextWaveDelayed());
            }
            else
            {
                OnAllWavesComplete?.Invoke();
            }
        }

        private IEnumerator StartNextWaveDelayed()
        {
            Debug.Log($"[WaveManager] 다음 웨이브까지 {timeBetweenWaves}초...");
            yield return new WaitForSeconds(timeBetweenWaves);
            StartNextWave();
        }

        private void HandlePlayerDeath()
        {
            StopAllCoroutines();
            isWaveActive = false;
            isPaused = true;

            Debug.Log($"[WaveManager] 게임 오버! 최종 웨이브: {currentWave}, 총 처치: {totalEnemiesKilled}");
            OnGameOver?.Invoke();
        }

        private WaveConfig GetCurrentWaveConfig()
        {
            if (currentWave > 0 && currentWave <= waveConfigs.Count)
            {
                return waveConfigs[currentWave - 1];
            }
            return new WaveConfig { enemyCount = baseEnemyCount, expBonus = 50, goldBonus = 20 };
        }

        private int GetCurrentWaveEnemyCount()
        {
            return GetCurrentWaveConfig().enemyCount;
        }

        /// <summary>
        /// 웨이브 시스템 리셋
        /// </summary>
        public void ResetWaves()
        {
            StopAllCoroutines();

            // 모든 적 제거
            foreach (var enemy in activeEnemies)
            {
                if (enemy != null)
                    Destroy(enemy.gameObject);
            }
            activeEnemies.Clear();

            currentWave = 0;
            enemiesAlive = 0;
            totalEnemiesKilled = 0;
            isWaveActive = false;
            isPaused = false;

            Debug.Log("[WaveManager] 웨이브 시스템 리셋됨");
        }

        /// <summary>
        /// 일시정지
        /// </summary>
        public void PauseWaves()
        {
            isPaused = true;
            Time.timeScale = 0f;
        }

        /// <summary>
        /// 재개
        /// </summary>
        public void ResumeWaves()
        {
            isPaused = false;
            Time.timeScale = 1f;
        }

        /// <summary>
        /// 모든 현재 적 처치
        /// </summary>
        public void KillAllCurrentEnemies()
        {
            foreach (var enemy in activeEnemies.ToArray())
            {
                if (enemy != null && enemy.IsAlive)
                {
                    enemy.TakeDamage(99999, DamageType.Physical);
                }
            }
        }

        /// <summary>
        /// 특정 웨이브로 스킵
        /// </summary>
        public void SkipToWave(int waveNumber)
        {
            StopAllCoroutines();
            KillAllCurrentEnemies();

            currentWave = waveNumber - 1;
            isWaveActive = false;

            StartNextWave();
        }
    }

    /// <summary>
    /// 웨이브 설정 데이터
    /// </summary>
    [Serializable]
    public class WaveConfig
    {
        public string waveName = "Wave";
        public int enemyCount = 5;
        public float spawnInterval = 1.5f;
        public bool isBossWave = false;
        public GameObject specificEnemyPrefab;  // null이면 랜덤
        public int expBonus = 50;
        public int goldBonus = 20;
    }
}
