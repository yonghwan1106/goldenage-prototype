using UnityEngine;
using System.Collections;

namespace GoldenAge.Core
{
    /// <summary>
    /// 슬로우모션/불릿타임 시스템
    /// </summary>
    public class SlowMotionSystem : Singleton<SlowMotionSystem>
    {
        [Header("설정")]
        [SerializeField] private float defaultSlowScale = 0.2f;
        [SerializeField] private float transitionDuration = 0.2f;
        [SerializeField] private bool affectAudio = true;

        [Header("후처리 효과")]
        [SerializeField] private bool enableVignette = true;
        [SerializeField] private float vignetteIntensity = 0.4f;
        [SerializeField] private bool enableChromaticAberration = true;
        [SerializeField] private float chromaticIntensity = 0.5f;

        private float originalTimeScale = 1f;
        private float originalFixedDeltaTime;
        private Coroutine slowMotionCoroutine;
        private bool isSlowMotion = false;

        public bool IsSlowMotion => isSlowMotion;
        public event System.Action OnSlowMotionStart;
        public event System.Action OnSlowMotionEnd;

        protected override void Awake()
        {
            base.Awake();
            originalFixedDeltaTime = Time.fixedDeltaTime;
        }

        /// <summary>
        /// 슬로우모션 시작
        /// </summary>
        public void StartSlowMotion(float timeScale = -1f, float duration = -1f)
        {
            if (timeScale < 0) timeScale = defaultSlowScale;

            if (slowMotionCoroutine != null)
            {
                StopCoroutine(slowMotionCoroutine);
            }

            slowMotionCoroutine = StartCoroutine(SlowMotionCoroutine(timeScale, duration));
        }

        /// <summary>
        /// 슬로우모션 종료
        /// </summary>
        public void StopSlowMotion()
        {
            if (slowMotionCoroutine != null)
            {
                StopCoroutine(slowMotionCoroutine);
            }

            StartCoroutine(RestoreTimeCoroutine());
        }

        /// <summary>
        /// 즉시 슬로우모션 적용
        /// </summary>
        public void ApplySlowMotionInstant(float timeScale)
        {
            Time.timeScale = timeScale;
            Time.fixedDeltaTime = originalFixedDeltaTime * timeScale;

            if (affectAudio)
            {
                AudioMixerController.Instance?.SetPitch(timeScale);
            }

            isSlowMotion = timeScale < 1f;

            if (isSlowMotion)
            {
                OnSlowMotionStart?.Invoke();
                ApplyPostProcessEffects(true);
            }
        }

        /// <summary>
        /// 즉시 시간 복원
        /// </summary>
        public void RestoreTimeInstant()
        {
            Time.timeScale = originalTimeScale;
            Time.fixedDeltaTime = originalFixedDeltaTime;

            if (affectAudio)
            {
                AudioMixerController.Instance?.SetPitch(1f);
            }

            isSlowMotion = false;
            OnSlowMotionEnd?.Invoke();
            ApplyPostProcessEffects(false);
        }

        private IEnumerator SlowMotionCoroutine(float targetScale, float duration)
        {
            isSlowMotion = true;
            OnSlowMotionStart?.Invoke();
            ApplyPostProcessEffects(true);

            // 전환 (unscaled time 사용)
            float elapsed = 0f;
            float startScale = Time.timeScale;

            while (elapsed < transitionDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / transitionDuration;
                float currentScale = Mathf.Lerp(startScale, targetScale, EaseOutCubic(t));

                Time.timeScale = currentScale;
                Time.fixedDeltaTime = originalFixedDeltaTime * currentScale;

                if (affectAudio)
                {
                    AudioMixerController.Instance?.SetPitch(currentScale);
                }

                yield return null;
            }

            Time.timeScale = targetScale;
            Time.fixedDeltaTime = originalFixedDeltaTime * targetScale;

            // 지속 시간이 있으면 대기 후 복원
            if (duration > 0)
            {
                yield return new WaitForSecondsRealtime(duration);
                yield return StartCoroutine(RestoreTimeCoroutine());
            }
        }

        private IEnumerator RestoreTimeCoroutine()
        {
            float elapsed = 0f;
            float startScale = Time.timeScale;

            while (elapsed < transitionDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / transitionDuration;
                float currentScale = Mathf.Lerp(startScale, originalTimeScale, EaseInCubic(t));

                Time.timeScale = currentScale;
                Time.fixedDeltaTime = originalFixedDeltaTime * currentScale;

                if (affectAudio)
                {
                    AudioMixerController.Instance?.SetPitch(currentScale);
                }

                yield return null;
            }

            Time.timeScale = originalTimeScale;
            Time.fixedDeltaTime = originalFixedDeltaTime;

            if (affectAudio)
            {
                AudioMixerController.Instance?.SetPitch(1f);
            }

            isSlowMotion = false;
            OnSlowMotionEnd?.Invoke();
            ApplyPostProcessEffects(false);
        }

        private void ApplyPostProcessEffects(bool enable)
        {
            // URP Post Processing Volume 제어
            // 실제 구현은 프로젝트의 후처리 설정에 따라 다름
            // Volume 컴포넌트를 찾아서 Vignette, Chromatic Aberration 조절

            // 예시 (Volume이 있을 경우):
            // var volume = FindObjectOfType<Volume>();
            // if (volume != null && volume.profile.TryGet(out Vignette vignette))
            // {
            //     vignette.intensity.value = enable ? vignetteIntensity : 0f;
            // }
        }

        private float EaseOutCubic(float t)
        {
            return 1f - Mathf.Pow(1f - t, 3f);
        }

        private float EaseInCubic(float t)
        {
            return t * t * t;
        }

        /// <summary>
        /// 히트스탑 효과 (짧은 정지)
        /// </summary>
        public void HitStop(float duration = 0.05f)
        {
            StartCoroutine(HitStopCoroutine(duration));
        }

        private IEnumerator HitStopCoroutine(float duration)
        {
            float savedTimeScale = Time.timeScale;
            Time.timeScale = 0f;

            yield return new WaitForSecondsRealtime(duration);

            Time.timeScale = savedTimeScale;
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                // 앱이 백그라운드로 갈 때 시간 복원
                RestoreTimeInstant();
            }
        }
    }
}
