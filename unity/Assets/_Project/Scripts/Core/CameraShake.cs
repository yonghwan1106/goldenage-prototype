using UnityEngine;
using System.Collections;

namespace GoldenAge.Core
{
    /// <summary>
    /// 카메라 흔들림 효과 시스템
    /// </summary>
    public class CameraShake : MonoBehaviour
    {
        public static CameraShake Instance { get; private set; }

        [Header("기본 설정")]
        [SerializeField] private float defaultDuration = 0.3f;
        [SerializeField] private float defaultMagnitude = 0.2f;
        [SerializeField] private float defaultRoughness = 10f;

        [Header("프리셋")]
        [SerializeField] private ShakePreset hitShake;
        [SerializeField] private ShakePreset explosionShake;
        [SerializeField] private ShakePreset lightShake;

        private Vector3 originalPosition;
        private Coroutine shakeCoroutine;
        private bool isShaking = false;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(this);
                return;
            }

            // 프리셋 기본값 설정
            if (hitShake == null)
                hitShake = new ShakePreset { duration = 0.15f, magnitude = 0.1f, roughness = 15f };
            if (explosionShake == null)
                explosionShake = new ShakePreset { duration = 0.4f, magnitude = 0.3f, roughness = 8f };
            if (lightShake == null)
                lightShake = new ShakePreset { duration = 0.1f, magnitude = 0.05f, roughness = 20f };
        }

        private void Start()
        {
            originalPosition = transform.localPosition;
        }

        /// <summary>
        /// 기본 쉐이크
        /// </summary>
        public void Shake()
        {
            Shake(defaultDuration, defaultMagnitude, defaultRoughness);
        }

        /// <summary>
        /// 커스텀 쉐이크
        /// </summary>
        public void Shake(float duration, float magnitude, float roughness = 10f)
        {
            if (!GameSettings.Instance?.Settings.cameraShake ?? true)
                return;

            if (shakeCoroutine != null)
            {
                StopCoroutine(shakeCoroutine);
            }

            shakeCoroutine = StartCoroutine(ShakeCoroutine(duration, magnitude, roughness));
        }

        /// <summary>
        /// 프리셋으로 쉐이크
        /// </summary>
        public void Shake(ShakeType type)
        {
            ShakePreset preset = type switch
            {
                ShakeType.Hit => hitShake,
                ShakeType.Explosion => explosionShake,
                ShakeType.Light => lightShake,
                _ => hitShake
            };

            Shake(preset.duration, preset.magnitude, preset.roughness);
        }

        /// <summary>
        /// 피격 쉐이크
        /// </summary>
        public void ShakeOnHit()
        {
            Shake(ShakeType.Hit);
        }

        /// <summary>
        /// 폭발 쉐이크
        /// </summary>
        public void ShakeOnExplosion()
        {
            Shake(ShakeType.Explosion);
        }

        private IEnumerator ShakeCoroutine(float duration, float magnitude, float roughness)
        {
            isShaking = true;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                // 펄린 노이즈 기반 부드러운 흔들림
                float x = (Mathf.PerlinNoise(Time.time * roughness, 0f) - 0.5f) * 2f;
                float y = (Mathf.PerlinNoise(0f, Time.time * roughness) - 0.5f) * 2f;

                // 시간에 따라 감소
                float currentMagnitude = magnitude * (1f - (elapsed / duration));

                transform.localPosition = originalPosition + new Vector3(x, y, 0) * currentMagnitude;

                elapsed += Time.deltaTime;
                yield return null;
            }

            transform.localPosition = originalPosition;
            isShaking = false;
            shakeCoroutine = null;
        }

        /// <summary>
        /// 즉시 중지
        /// </summary>
        public void StopShake()
        {
            if (shakeCoroutine != null)
            {
                StopCoroutine(shakeCoroutine);
                shakeCoroutine = null;
            }

            transform.localPosition = originalPosition;
            isShaking = false;
        }

        /// <summary>
        /// 원래 위치 업데이트 (카메라 이동 시)
        /// </summary>
        public void UpdateOriginalPosition()
        {
            if (!isShaking)
            {
                originalPosition = transform.localPosition;
            }
        }
    }

    [System.Serializable]
    public class ShakePreset
    {
        public float duration = 0.2f;
        public float magnitude = 0.1f;
        public float roughness = 10f;
    }

    public enum ShakeType
    {
        Hit,
        Explosion,
        Light
    }
}
