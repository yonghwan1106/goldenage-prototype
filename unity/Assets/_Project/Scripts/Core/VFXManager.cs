using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace GoldenAge.Core
{
    /// <summary>
    /// VFX 스폰 및 관리 매니저
    /// </summary>
    public class VFXManager : Singleton<VFXManager>
    {
        [Header("이펙트 프리팹")]
        [SerializeField] private GameObject teslaShockVFX;
        [SerializeField] private GameObject etherWaveVFX;
        [SerializeField] private GameObject fusionBlastVFX;
        [SerializeField] private GameObject hitVFX;
        [SerializeField] private GameObject levelUpVFX;

        [Header("풀링 설정")]
        [SerializeField] private int initialPoolSize = 5;

        // 오브젝트 풀링
        private Dictionary<string, Queue<ParticleSystem>> vfxPools = new Dictionary<string, Queue<ParticleSystem>>();
        private Dictionary<string, GameObject> prefabMap = new Dictionary<string, GameObject>();

        protected override void Awake()
        {
            base.Awake();
            InitializePrefabMap();
        }

        private void InitializePrefabMap()
        {
            if (teslaShockVFX != null) prefabMap["TeslaShock"] = teslaShockVFX;
            if (etherWaveVFX != null) prefabMap["EtherWave"] = etherWaveVFX;
            if (fusionBlastVFX != null) prefabMap["FusionBlast"] = fusionBlastVFX;
            if (hitVFX != null) prefabMap["Hit"] = hitVFX;
            if (levelUpVFX != null) prefabMap["LevelUp"] = levelUpVFX;
        }

        /// <summary>
        /// VFX 스폰 (이름으로)
        /// </summary>
        public void SpawnVFX(string vfxName, Vector3 position)
        {
            SpawnVFX(vfxName, position, Quaternion.identity);
        }

        /// <summary>
        /// VFX 스폰 (이름 + 회전)
        /// </summary>
        public void SpawnVFX(string vfxName, Vector3 position, Quaternion rotation)
        {
            if (!prefabMap.TryGetValue(vfxName, out GameObject prefab))
            {
                Debug.LogWarning($"[VFXManager] VFX not found: {vfxName}");
                return;
            }

            SpawnVFX(prefab, position, rotation);
        }

        /// <summary>
        /// VFX 스폰 (프리팹 직접 지정)
        /// </summary>
        public void SpawnVFX(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            if (prefab == null) return;

            string key = prefab.name;

            // 풀에서 가져오기
            ParticleSystem ps = GetFromPool(key);

            if (ps == null)
            {
                // 새로 생성
                GameObject obj = Instantiate(prefab, position, rotation);
                ps = obj.GetComponent<ParticleSystem>();

                if (ps == null)
                {
                    // ParticleSystem이 없으면 일정 시간 후 파괴
                    Destroy(obj, 3f);
                    return;
                }
            }
            else
            {
                ps.transform.position = position;
                ps.transform.rotation = rotation;
                ps.gameObject.SetActive(true);
            }

            ps.Play();

            // 재생 완료 후 풀로 반환
            float duration = ps.main.duration + ps.main.startLifetime.constantMax;
            StartCoroutine(ReturnToPool(ps, key, duration + 0.5f));
        }

        /// <summary>
        /// VFX 스폰 (Transform 부착)
        /// </summary>
        public GameObject SpawnVFXAttached(GameObject prefab, Transform parent)
        {
            if (prefab == null || parent == null) return null;

            GameObject obj = Instantiate(prefab, parent);
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localRotation = Quaternion.identity;

            return obj;
        }

        private ParticleSystem GetFromPool(string key)
        {
            if (vfxPools.TryGetValue(key, out var pool) && pool.Count > 0)
            {
                return pool.Dequeue();
            }
            return null;
        }

        private IEnumerator ReturnToPool(ParticleSystem ps, string key, float delay)
        {
            yield return new WaitForSeconds(delay);

            if (ps == null) yield break;

            ps.Stop();
            ps.gameObject.SetActive(false);

            if (!vfxPools.ContainsKey(key))
            {
                vfxPools[key] = new Queue<ParticleSystem>();
            }

            vfxPools[key].Enqueue(ps);
        }

        /// <summary>
        /// 프리팹 등록 (런타임)
        /// </summary>
        public void RegisterPrefab(string name, GameObject prefab)
        {
            if (!string.IsNullOrEmpty(name) && prefab != null)
            {
                prefabMap[name] = prefab;
            }
        }

        /// <summary>
        /// 모든 풀 정리
        /// </summary>
        public void ClearAllPools()
        {
            foreach (var pool in vfxPools.Values)
            {
                while (pool.Count > 0)
                {
                    var ps = pool.Dequeue();
                    if (ps != null)
                    {
                        Destroy(ps.gameObject);
                    }
                }
            }
            vfxPools.Clear();
        }
    }
}
