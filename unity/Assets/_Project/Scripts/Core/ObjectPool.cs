using UnityEngine;
using System.Collections.Generic;

namespace GoldenAge.Core
{
    /// <summary>
    /// 범용 오브젝트 풀링 시스템
    /// </summary>
    public class ObjectPool : Singleton<ObjectPool>
    {
        [System.Serializable]
        public class PoolConfig
        {
            public string poolName;
            public GameObject prefab;
            public int initialSize = 10;
            public int maxSize = 50;
            public bool expandable = true;
        }

        [Header("풀 설정")]
        [SerializeField] private PoolConfig[] poolConfigs;

        private Dictionary<string, Queue<GameObject>> pools = new Dictionary<string, Queue<GameObject>>();
        private Dictionary<string, PoolConfig> configMap = new Dictionary<string, PoolConfig>();
        private Dictionary<string, Transform> poolContainers = new Dictionary<string, Transform>();

        protected override void Awake()
        {
            base.Awake();
            InitializePools();
        }

        private void InitializePools()
        {
            if (poolConfigs == null) return;

            foreach (var config in poolConfigs)
            {
                if (config.prefab == null) continue;

                CreatePool(config);
            }
        }

        private void CreatePool(PoolConfig config)
        {
            string key = config.poolName;
            if (string.IsNullOrEmpty(key))
                key = config.prefab.name;

            if (pools.ContainsKey(key))
            {
                Debug.LogWarning($"[ObjectPool] 풀 '{key}' 이미 존재함");
                return;
            }

            // 컨테이너 생성
            GameObject container = new GameObject($"Pool_{key}");
            container.transform.SetParent(transform);
            poolContainers[key] = container.transform;

            // 풀 생성
            pools[key] = new Queue<GameObject>();
            configMap[key] = config;

            // 초기 오브젝트 생성
            for (int i = 0; i < config.initialSize; i++)
            {
                CreateNewObject(key, config.prefab);
            }

            Debug.Log($"[ObjectPool] 풀 '{key}' 생성됨 (초기: {config.initialSize})");
        }

        private GameObject CreateNewObject(string key, GameObject prefab)
        {
            GameObject obj = Instantiate(prefab, poolContainers[key]);
            obj.SetActive(false);

            PooledObject pooledObj = obj.GetComponent<PooledObject>();
            if (pooledObj == null)
                pooledObj = obj.AddComponent<PooledObject>();

            pooledObj.PoolKey = key;

            pools[key].Enqueue(obj);
            return obj;
        }

        /// <summary>
        /// 풀에서 오브젝트 가져오기
        /// </summary>
        public GameObject Get(string poolName, Vector3 position, Quaternion rotation)
        {
            if (!pools.ContainsKey(poolName))
            {
                Debug.LogWarning($"[ObjectPool] 풀 '{poolName}' 없음");
                return null;
            }

            Queue<GameObject> pool = pools[poolName];
            PoolConfig config = configMap[poolName];

            GameObject obj = null;

            // 사용 가능한 오브젝트 찾기
            while (pool.Count > 0 && obj == null)
            {
                obj = pool.Dequeue();
                if (obj == null) continue;
            }

            // 없으면 새로 생성 (확장 가능한 경우)
            if (obj == null && config.expandable)
            {
                obj = CreateNewObject(poolName, config.prefab);
                pool.Dequeue(); // 방금 추가한 것 꺼내기
            }

            if (obj == null)
            {
                Debug.LogWarning($"[ObjectPool] 풀 '{poolName}' 가득 참");
                return null;
            }

            // 활성화
            obj.transform.position = position;
            obj.transform.rotation = rotation;
            obj.SetActive(true);

            // 초기화 콜백
            PooledObject pooledObj = obj.GetComponent<PooledObject>();
            pooledObj?.OnSpawn();

            return obj;
        }

        /// <summary>
        /// 풀에서 오브젝트 가져오기 (프리팹 이름 사용)
        /// </summary>
        public GameObject Get(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            return Get(prefab.name, position, rotation);
        }

        /// <summary>
        /// 오브젝트를 풀로 반환
        /// </summary>
        public void Return(GameObject obj)
        {
            if (obj == null) return;

            PooledObject pooledObj = obj.GetComponent<PooledObject>();
            if (pooledObj == null)
            {
                Debug.LogWarning($"[ObjectPool] PooledObject 컴포넌트 없음: {obj.name}");
                Destroy(obj);
                return;
            }

            string key = pooledObj.PoolKey;
            if (!pools.ContainsKey(key))
            {
                Debug.LogWarning($"[ObjectPool] 풀 '{key}' 없음");
                Destroy(obj);
                return;
            }

            // 비활성화 콜백
            pooledObj.OnDespawn();

            obj.SetActive(false);
            obj.transform.SetParent(poolContainers[key]);
            pools[key].Enqueue(obj);
        }

        /// <summary>
        /// 지연 반환
        /// </summary>
        public void ReturnDelayed(GameObject obj, float delay)
        {
            if (obj == null) return;
            StartCoroutine(ReturnDelayedCoroutine(obj, delay));
        }

        private System.Collections.IEnumerator ReturnDelayedCoroutine(GameObject obj, float delay)
        {
            yield return new WaitForSeconds(delay);
            Return(obj);
        }

        /// <summary>
        /// 런타임에 풀 등록
        /// </summary>
        public void RegisterPool(string poolName, GameObject prefab, int initialSize = 10, int maxSize = 50)
        {
            if (pools.ContainsKey(poolName)) return;

            PoolConfig config = new PoolConfig
            {
                poolName = poolName,
                prefab = prefab,
                initialSize = initialSize,
                maxSize = maxSize,
                expandable = true
            };

            CreatePool(config);
        }

        /// <summary>
        /// 풀 정리
        /// </summary>
        public void ClearPool(string poolName)
        {
            if (!pools.ContainsKey(poolName)) return;

            Queue<GameObject> pool = pools[poolName];
            while (pool.Count > 0)
            {
                GameObject obj = pool.Dequeue();
                if (obj != null) Destroy(obj);
            }

            pools.Remove(poolName);
            configMap.Remove(poolName);

            if (poolContainers.ContainsKey(poolName))
            {
                Destroy(poolContainers[poolName].gameObject);
                poolContainers.Remove(poolName);
            }
        }

        /// <summary>
        /// 모든 풀 정리
        /// </summary>
        public void ClearAllPools()
        {
            List<string> keys = new List<string>(pools.Keys);
            foreach (string key in keys)
            {
                ClearPool(key);
            }
        }
    }

    /// <summary>
    /// 풀링된 오브젝트 컴포넌트
    /// </summary>
    public class PooledObject : MonoBehaviour
    {
        public string PoolKey { get; set; }

        public event System.Action OnSpawnEvent;
        public event System.Action OnDespawnEvent;

        public virtual void OnSpawn()
        {
            OnSpawnEvent?.Invoke();
        }

        public virtual void OnDespawn()
        {
            OnDespawnEvent?.Invoke();
        }

        /// <summary>
        /// 풀로 반환
        /// </summary>
        public void ReturnToPool()
        {
            ObjectPool.Instance?.Return(gameObject);
        }

        /// <summary>
        /// 지연 반환
        /// </summary>
        public void ReturnToPool(float delay)
        {
            ObjectPool.Instance?.ReturnDelayed(gameObject, delay);
        }
    }
}
