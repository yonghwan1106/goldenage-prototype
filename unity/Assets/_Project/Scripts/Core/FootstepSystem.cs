using UnityEngine;
using System.Collections.Generic;

namespace GoldenAge.Core
{
    /// <summary>
    /// 발소리 시스템
    /// </summary>
    public class FootstepSystem : MonoBehaviour
    {
        [Header("오디오 소스")]
        [SerializeField] private AudioSource footstepSource;

        [Header("설정")]
        [SerializeField] private float walkStepInterval = 0.5f;
        [SerializeField] private float runStepInterval = 0.3f;
        [SerializeField] private float volumeVariation = 0.1f;
        [SerializeField] private float pitchVariation = 0.1f;

        [Header("지면 감지")]
        [SerializeField] private float groundCheckDistance = 0.3f;
        [SerializeField] private LayerMask groundLayer;

        [Header("발소리 데이터")]
        [SerializeField] private FootstepData[] footstepDataList;

        private Dictionary<SurfaceType, FootstepData> footstepMap;
        private float stepTimer;
        private bool isMoving;
        private bool isRunning;
        private SurfaceType currentSurface = SurfaceType.Stone;

        private void Awake()
        {
            // 딕셔너리 초기화
            footstepMap = new Dictionary<SurfaceType, FootstepData>();

            if (footstepDataList != null)
            {
                foreach (var data in footstepDataList)
                {
                    if (!footstepMap.ContainsKey(data.surfaceType))
                    {
                        footstepMap[data.surfaceType] = data;
                    }
                }
            }

            if (footstepSource == null)
            {
                footstepSource = GetComponent<AudioSource>();
                if (footstepSource == null)
                {
                    footstepSource = gameObject.AddComponent<AudioSource>();
                    footstepSource.spatialBlend = 1f;
                    footstepSource.playOnAwake = false;
                }
            }
        }

        private void Update()
        {
            if (!isMoving)
            {
                stepTimer = 0f;
                return;
            }

            // 지면 타입 감지
            DetectSurface();

            // 걸음 타이머
            float interval = isRunning ? runStepInterval : walkStepInterval;
            stepTimer += Time.deltaTime;

            if (stepTimer >= interval)
            {
                PlayFootstep();
                stepTimer = 0f;
            }
        }

        private void DetectSurface()
        {
            if (Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, out RaycastHit hit, groundCheckDistance, groundLayer))
            {
                // 태그나 머티리얼로 표면 타입 결정
                SurfaceType detected = GetSurfaceType(hit);
                currentSurface = detected;
            }
        }

        private SurfaceType GetSurfaceType(RaycastHit hit)
        {
            // 태그로 확인
            string tag = hit.collider.tag;
            switch (tag)
            {
                case "Wood": return SurfaceType.Wood;
                case "Metal": return SurfaceType.Metal;
                case "Water": return SurfaceType.Water;
                case "Grass": return SurfaceType.Grass;
                case "Concrete": return SurfaceType.Concrete;
            }

            // 머티리얼 이름으로 확인
            Renderer renderer = hit.collider.GetComponent<Renderer>();
            if (renderer != null && renderer.sharedMaterial != null)
            {
                string matName = renderer.sharedMaterial.name.ToLower();

                if (matName.Contains("wood")) return SurfaceType.Wood;
                if (matName.Contains("metal")) return SurfaceType.Metal;
                if (matName.Contains("water")) return SurfaceType.Water;
                if (matName.Contains("grass")) return SurfaceType.Grass;
                if (matName.Contains("concrete")) return SurfaceType.Concrete;
                if (matName.Contains("cobble") || matName.Contains("stone")) return SurfaceType.Stone;
            }

            return SurfaceType.Stone; // 기본값
        }

        private void PlayFootstep()
        {
            if (footstepSource == null) return;

            AudioClip clip = GetFootstepClip(currentSurface);
            if (clip == null) return;

            // 볼륨/피치 변화
            float volume = 1f + Random.Range(-volumeVariation, volumeVariation);
            float pitch = 1f + Random.Range(-pitchVariation, pitchVariation);

            if (isRunning)
            {
                volume *= 1.2f;
                pitch *= 1.1f;
            }

            footstepSource.pitch = pitch;
            footstepSource.PlayOneShot(clip, volume);
        }

        private AudioClip GetFootstepClip(SurfaceType surface)
        {
            if (footstepMap.TryGetValue(surface, out FootstepData data))
            {
                if (data.clips != null && data.clips.Length > 0)
                {
                    return data.clips[Random.Range(0, data.clips.Length)];
                }
            }

            // 기본 발소리
            if (footstepMap.TryGetValue(SurfaceType.Stone, out FootstepData defaultData))
            {
                if (defaultData.clips != null && defaultData.clips.Length > 0)
                {
                    return defaultData.clips[Random.Range(0, defaultData.clips.Length)];
                }
            }

            return null;
        }

        /// <summary>
        /// 이동 상태 설정 (PlayerMovement에서 호출)
        /// </summary>
        public void SetMoving(bool moving, bool running = false)
        {
            isMoving = moving;
            isRunning = running;
        }

        /// <summary>
        /// 즉시 발소리 재생
        /// </summary>
        public void PlayStepNow()
        {
            PlayFootstep();
        }

        /// <summary>
        /// 착지 소리 재생
        /// </summary>
        public void PlayLanding(float fallSpeed)
        {
            if (footstepSource == null) return;

            AudioClip clip = GetFootstepClip(currentSurface);
            if (clip == null) return;

            // 낙하 속도에 따른 볼륨
            float volume = Mathf.Clamp(fallSpeed / 10f, 0.5f, 1.5f);

            footstepSource.pitch = 0.8f;
            footstepSource.PlayOneShot(clip, volume);
        }
    }

    [System.Serializable]
    public class FootstepData
    {
        public SurfaceType surfaceType;
        public AudioClip[] clips;
        [Range(0f, 1f)] public float volumeMultiplier = 1f;
    }

    public enum SurfaceType
    {
        Stone,      // 돌/조약돌
        Concrete,   // 콘크리트
        Wood,       // 나무
        Metal,      // 금속
        Water,      // 물/웅덩이
        Grass,      // 풀/흙
        Carpet      // 카펫
    }
}
