using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace GoldenAge.Combat
{
    /// <summary>
    /// 적 스폰 관리 시스템
    /// </summary>
    public class EnemySpawner : MonoBehaviour
    {
        [Header("스폰 설정")]
        [SerializeField] private GameObject[] enemyPrefabs;
        [SerializeField] private Transform[] spawnPoints;
        [SerializeField] private float spawnRadius = 2f;

        [Header("웨이브 설정")]
        [SerializeField] private WaveData[] waves;
        [SerializeField] private float timeBetweenWaves = 5f;
        [SerializeField] private bool autoStartWaves = true;

        [Header("제한")]
        [SerializeField] private int maxEnemiesAlive = 10;

        // 상태
        private int currentWaveIndex = 0;
        private int enemiesAliveCount = 0;
        private int enemiesSpawnedInWave = 0;
        private bool isSpawning = false;
        private bool allWavesComplete = false;

        // 이벤트
        public System.Action<int> OnWaveStart;
        public System.Action<int> OnWaveComplete;
        public System.Action OnAllWavesComplete;

        private List<GameObject> activeEnemies = new List<GameObject>();

        private void Start()
        {
            if (autoStartWaves && waves.Length > 0)
            {
                StartCoroutine(StartWaveSystem());
            }
        }

        private IEnumerator StartWaveSystem()
        {
            yield return new WaitForSeconds(2f); // 초기 대기

            while (currentWaveIndex < waves.Length)
            {
                yield return StartCoroutine(SpawnWave(waves[currentWaveIndex]));

                // 웨이브 완료 대기
                yield return new WaitUntil(() => enemiesAliveCount == 0);

                OnWaveComplete?.Invoke(currentWaveIndex);
                Debug.Log($"[Spawner] 웨이브 {currentWaveIndex + 1} 완료!");

                currentWaveIndex++;

                if (currentWaveIndex < waves.Length)
                {
                    yield return new WaitForSeconds(timeBetweenWaves);
                }
            }

            allWavesComplete = true;
            OnAllWavesComplete?.Invoke();
            Debug.Log("[Spawner] 모든 웨이브 완료!");
        }

        private IEnumerator SpawnWave(WaveData wave)
        {
            isSpawning = true;
            enemiesSpawnedInWave = 0;

            OnWaveStart?.Invoke(currentWaveIndex);
            Debug.Log($"[Spawner] 웨이브 {currentWaveIndex + 1} 시작! 적 수: {wave.enemyCount}");

            for (int i = 0; i < wave.enemyCount; i++)
            {
                // 최대 수 제한 대기
                yield return new WaitUntil(() => enemiesAliveCount < maxEnemiesAlive);

                SpawnEnemy(wave);
                enemiesSpawnedInWave++;

                yield return new WaitForSeconds(wave.spawnInterval);
            }

            isSpawning = false;
        }

        private void SpawnEnemy(WaveData wave)
        {
            // 프리팹 선택
            GameObject prefab = wave.enemyPrefab != null
                ? wave.enemyPrefab
                : enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];

            if (prefab == null)
            {
                Debug.LogWarning("[Spawner] 적 프리팹이 없습니다.");
                return;
            }

            // 스폰 위치 선택
            Vector3 spawnPos = GetSpawnPosition();

            // 스폰
            GameObject enemy = Instantiate(prefab, spawnPos, Quaternion.identity);
            activeEnemies.Add(enemy);
            enemiesAliveCount++;

            // 사망 이벤트 연결
            EnemyAI enemyAI = enemy.GetComponent<EnemyAI>();
            if (enemyAI != null)
            {
                enemyAI.OnDeath += () => OnEnemyDeath(enemy);
            }
        }

        private Vector3 GetSpawnPosition()
        {
            if (spawnPoints == null || spawnPoints.Length == 0)
            {
                // 스포너 주변 랜덤 위치
                Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
                return transform.position + new Vector3(randomCircle.x, 0, randomCircle.y);
            }

            // 스폰 포인트 중 랜덤 선택
            Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
            Vector2 offset = Random.insideUnitCircle * spawnRadius;
            return spawnPoint.position + new Vector3(offset.x, 0, offset.y);
        }

        private void OnEnemyDeath(GameObject enemy)
        {
            enemiesAliveCount--;
            activeEnemies.Remove(enemy);
        }

        /// <summary>
        /// 수동으로 웨이브 시작
        /// </summary>
        public void StartNextWave()
        {
            if (!isSpawning && currentWaveIndex < waves.Length)
            {
                StartCoroutine(SpawnWave(waves[currentWaveIndex]));
            }
        }

        /// <summary>
        /// 모든 적 제거
        /// </summary>
        public void ClearAllEnemies()
        {
            foreach (var enemy in activeEnemies)
            {
                if (enemy != null)
                    Destroy(enemy);
            }
            activeEnemies.Clear();
            enemiesAliveCount = 0;
        }

        /// <summary>
        /// 스포너 리셋
        /// </summary>
        public void ResetSpawner()
        {
            StopAllCoroutines();
            ClearAllEnemies();
            currentWaveIndex = 0;
            isSpawning = false;
            allWavesComplete = false;
        }

        // Gizmos
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, spawnRadius);

            if (spawnPoints != null)
            {
                foreach (var point in spawnPoints)
                {
                    if (point != null)
                    {
                        Gizmos.DrawWireSphere(point.position, 0.5f);
                        Gizmos.DrawLine(transform.position, point.position);
                    }
                }
            }
        }
    }

    [System.Serializable]
    public class WaveData
    {
        public string waveName = "Wave";
        public int enemyCount = 5;
        public float spawnInterval = 1f;
        public GameObject enemyPrefab; // null이면 기본 프리팹 사용

        [Header("보너스")]
        public int experienceBonus = 50;
        public GameObject[] bonusDrops;
    }
}
