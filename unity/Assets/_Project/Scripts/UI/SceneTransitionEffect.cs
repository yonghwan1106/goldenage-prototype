using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace GoldenAge.UI
{
    /// <summary>
    /// 씬 전환 시각 효과 시스템
    /// </summary>
    public class SceneTransitionEffect : MonoBehaviour
    {
        public static SceneTransitionEffect Instance { get; private set; }

        [Header("UI 요소")]
        [SerializeField] private Canvas transitionCanvas;
        [SerializeField] private Image fadeImage;
        [SerializeField] private Image wipeImage;
        [SerializeField] private RawImage circleWipeImage;

        [Header("기본 설정")]
        [SerializeField] private TransitionType defaultType = TransitionType.Fade;
        [SerializeField] private float defaultDuration = 0.5f;
        [SerializeField] private Color fadeColor = Color.black;

        [Header("셰이더")]
        [SerializeField] private Material wipeMaterial;
        [SerializeField] private Material circleWipeMaterial;

        private Coroutine transitionCoroutine;
        private bool isTransitioning = false;

        public bool IsTransitioning => isTransitioning;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            InitializeUI();
        }

        private void InitializeUI()
        {
            // 캔버스 설정
            if (transitionCanvas != null)
            {
                transitionCanvas.sortingOrder = 9999; // 최상위
            }

            // 이미지 초기화
            if (fadeImage != null)
            {
                fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0f);
                fadeImage.raycastTarget = false;
                fadeImage.gameObject.SetActive(false);
            }

            if (wipeImage != null)
            {
                wipeImage.gameObject.SetActive(false);
            }

            if (circleWipeImage != null)
            {
                circleWipeImage.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// 전환 시작 (화면 가림)
        /// </summary>
        public void TransitionIn(TransitionType type = TransitionType.Default, float duration = -1f)
        {
            if (type == TransitionType.Default) type = defaultType;
            if (duration < 0) duration = defaultDuration;

            if (transitionCoroutine != null)
            {
                StopCoroutine(transitionCoroutine);
            }

            transitionCoroutine = StartCoroutine(TransitionInCoroutine(type, duration));
        }

        /// <summary>
        /// 전환 종료 (화면 드러남)
        /// </summary>
        public void TransitionOut(TransitionType type = TransitionType.Default, float duration = -1f)
        {
            if (type == TransitionType.Default) type = defaultType;
            if (duration < 0) duration = defaultDuration;

            if (transitionCoroutine != null)
            {
                StopCoroutine(transitionCoroutine);
            }

            transitionCoroutine = StartCoroutine(TransitionOutCoroutine(type, duration));
        }

        /// <summary>
        /// 전체 전환 (In -> callback -> Out)
        /// </summary>
        public void DoTransition(System.Action onMidpoint, TransitionType type = TransitionType.Default, float duration = -1f)
        {
            if (type == TransitionType.Default) type = defaultType;
            if (duration < 0) duration = defaultDuration;

            if (transitionCoroutine != null)
            {
                StopCoroutine(transitionCoroutine);
            }

            transitionCoroutine = StartCoroutine(FullTransitionCoroutine(type, duration, onMidpoint));
        }

        private IEnumerator TransitionInCoroutine(TransitionType type, float duration)
        {
            isTransitioning = true;

            switch (type)
            {
                case TransitionType.Fade:
                    yield return StartCoroutine(FadeIn(duration));
                    break;
                case TransitionType.WipeLeft:
                    yield return StartCoroutine(WipeIn(duration, Vector2.left));
                    break;
                case TransitionType.WipeRight:
                    yield return StartCoroutine(WipeIn(duration, Vector2.right));
                    break;
                case TransitionType.WipeUp:
                    yield return StartCoroutine(WipeIn(duration, Vector2.up));
                    break;
                case TransitionType.WipeDown:
                    yield return StartCoroutine(WipeIn(duration, Vector2.down));
                    break;
                case TransitionType.CircleClose:
                    yield return StartCoroutine(CircleWipeIn(duration));
                    break;
            }

            isTransitioning = false;
        }

        private IEnumerator TransitionOutCoroutine(TransitionType type, float duration)
        {
            isTransitioning = true;

            switch (type)
            {
                case TransitionType.Fade:
                    yield return StartCoroutine(FadeOut(duration));
                    break;
                case TransitionType.WipeLeft:
                    yield return StartCoroutine(WipeOut(duration, Vector2.left));
                    break;
                case TransitionType.WipeRight:
                    yield return StartCoroutine(WipeOut(duration, Vector2.right));
                    break;
                case TransitionType.WipeUp:
                    yield return StartCoroutine(WipeOut(duration, Vector2.up));
                    break;
                case TransitionType.WipeDown:
                    yield return StartCoroutine(WipeOut(duration, Vector2.down));
                    break;
                case TransitionType.CircleClose:
                    yield return StartCoroutine(CircleWipeOut(duration));
                    break;
            }

            isTransitioning = false;
        }

        private IEnumerator FullTransitionCoroutine(TransitionType type, float duration, System.Action onMidpoint)
        {
            isTransitioning = true;

            // In
            yield return StartCoroutine(TransitionInCoroutine(type, duration));

            // 콜백 실행
            onMidpoint?.Invoke();

            // 잠시 대기 (씬 로드 등)
            yield return new WaitForSeconds(0.1f);

            // Out
            yield return StartCoroutine(TransitionOutCoroutine(type, duration));

            isTransitioning = false;
        }

        #region Fade Effect

        private IEnumerator FadeIn(float duration)
        {
            if (fadeImage == null) yield break;

            fadeImage.gameObject.SetActive(true);
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float alpha = Mathf.Clamp01(elapsed / duration);
                fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, alpha);
                yield return null;
            }

            fadeImage.color = fadeColor;
        }

        private IEnumerator FadeOut(float duration)
        {
            if (fadeImage == null) yield break;

            fadeImage.gameObject.SetActive(true);
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float alpha = 1f - Mathf.Clamp01(elapsed / duration);
                fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, alpha);
                yield return null;
            }

            fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0f);
            fadeImage.gameObject.SetActive(false);
        }

        #endregion

        #region Wipe Effect

        private IEnumerator WipeIn(float duration, Vector2 direction)
        {
            if (wipeImage == null) yield break;

            wipeImage.gameObject.SetActive(true);
            RectTransform rt = wipeImage.rectTransform;
            Vector2 screenSize = new Vector2(Screen.width, Screen.height);

            Vector2 startPos = -direction * screenSize;
            Vector2 endPos = Vector2.zero;

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = EaseInOutCubic(elapsed / duration);
                rt.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
                yield return null;
            }

            rt.anchoredPosition = endPos;
        }

        private IEnumerator WipeOut(float duration, Vector2 direction)
        {
            if (wipeImage == null) yield break;

            RectTransform rt = wipeImage.rectTransform;
            Vector2 screenSize = new Vector2(Screen.width, Screen.height);

            Vector2 startPos = Vector2.zero;
            Vector2 endPos = direction * screenSize;

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = EaseInOutCubic(elapsed / duration);
                rt.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
                yield return null;
            }

            rt.anchoredPosition = endPos;
            wipeImage.gameObject.SetActive(false);
        }

        #endregion

        #region Circle Wipe Effect

        private IEnumerator CircleWipeIn(float duration)
        {
            if (circleWipeMaterial == null) yield break;

            circleWipeImage?.gameObject.SetActive(true);
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float progress = Mathf.Clamp01(elapsed / duration);
                circleWipeMaterial.SetFloat("_Progress", 1f - progress);
                yield return null;
            }

            circleWipeMaterial.SetFloat("_Progress", 0f);
        }

        private IEnumerator CircleWipeOut(float duration)
        {
            if (circleWipeMaterial == null) yield break;

            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float progress = Mathf.Clamp01(elapsed / duration);
                circleWipeMaterial.SetFloat("_Progress", progress);
                yield return null;
            }

            circleWipeMaterial.SetFloat("_Progress", 1f);
            circleWipeImage?.gameObject.SetActive(false);
        }

        #endregion

        private float EaseInOutCubic(float t)
        {
            return t < 0.5f
                ? 4f * t * t * t
                : 1f - Mathf.Pow(-2f * t + 2f, 3f) / 2f;
        }

        /// <summary>
        /// 페이드 색상 설정
        /// </summary>
        public void SetFadeColor(Color color)
        {
            fadeColor = color;
        }
    }

    public enum TransitionType
    {
        Default,
        Fade,
        WipeLeft,
        WipeRight,
        WipeUp,
        WipeDown,
        CircleClose
    }
}
