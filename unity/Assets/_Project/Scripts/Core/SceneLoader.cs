using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

namespace GoldenAge.Core
{
    /// <summary>
    /// 씬 로딩 및 전환 관리
    /// </summary>
    public class SceneLoader : Singleton<SceneLoader>
    {
        [Header("로딩 화면")]
        [SerializeField] private GameObject loadingScreen;
        [SerializeField] private UnityEngine.UI.Slider progressBar;
        [SerializeField] private TMPro.TextMeshProUGUI progressText;
        [SerializeField] private TMPro.TextMeshProUGUI tipText;

        [Header("팁")]
        [SerializeField] private string[] loadingTips = new string[]
        {
            "테슬라 충격기와 에테르 파동을 연속으로 사용하면 차원 전격이 발동됩니다!",
            "E키로 NPC와 대화하고 단서를 수집하세요.",
            "Q키: 테슬라 충격기, R키: 에테르 파동",
            "적에게 피격 시 잠시 무적 시간이 있습니다.",
            "스피크이지에서 정보원을 찾아보세요."
        };

        [Header("전환 효과")]
        [SerializeField] private float fadeTime = 0.5f;
        [SerializeField] private CanvasGroup fadeOverlay;

        private bool isLoading = false;

        protected override void Awake()
        {
            base.Awake();
            DontDestroyOnLoad(gameObject);

            if (loadingScreen != null)
                loadingScreen.SetActive(false);
        }

        /// <summary>
        /// 씬 로드 (이름)
        /// </summary>
        public void LoadScene(string sceneName)
        {
            if (isLoading) return;
            StartCoroutine(LoadSceneAsync(sceneName));
        }

        /// <summary>
        /// 씬 로드 (인덱스)
        /// </summary>
        public void LoadScene(int sceneIndex)
        {
            if (isLoading) return;
            StartCoroutine(LoadSceneAsync(sceneIndex));
        }

        /// <summary>
        /// 현재 씬 다시 로드
        /// </summary>
        public void ReloadCurrentScene()
        {
            LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        private IEnumerator LoadSceneAsync(string sceneName)
        {
            yield return StartCoroutine(LoadSceneRoutine(() => SceneManager.LoadSceneAsync(sceneName)));
        }

        private IEnumerator LoadSceneAsync(int sceneIndex)
        {
            yield return StartCoroutine(LoadSceneRoutine(() => SceneManager.LoadSceneAsync(sceneIndex)));
        }

        private IEnumerator LoadSceneRoutine(System.Func<AsyncOperation> loadOperation)
        {
            isLoading = true;

            // 페이드 아웃
            yield return StartCoroutine(Fade(1f));

            // 로딩 화면 표시
            ShowLoadingScreen();

            // 팁 표시
            if (tipText != null && loadingTips.Length > 0)
            {
                tipText.text = loadingTips[Random.Range(0, loadingTips.Length)];
            }

            // 비동기 로딩 시작
            AsyncOperation operation = loadOperation();
            operation.allowSceneActivation = false;

            // 로딩 진행률 업데이트
            while (operation.progress < 0.9f)
            {
                UpdateProgress(operation.progress / 0.9f);
                yield return null;
            }

            // 완료
            UpdateProgress(1f);
            yield return new WaitForSeconds(0.5f);

            // 씬 활성화
            operation.allowSceneActivation = true;

            // 씬 로드 완료 대기
            yield return new WaitUntil(() => operation.isDone);

            // 로딩 화면 숨기기
            HideLoadingScreen();

            // 페이드 인
            yield return StartCoroutine(Fade(0f));

            isLoading = false;
        }

        private void ShowLoadingScreen()
        {
            if (loadingScreen != null)
                loadingScreen.SetActive(true);
        }

        private void HideLoadingScreen()
        {
            if (loadingScreen != null)
                loadingScreen.SetActive(false);
        }

        private void UpdateProgress(float progress)
        {
            if (progressBar != null)
                progressBar.value = progress;

            if (progressText != null)
                progressText.text = $"{Mathf.RoundToInt(progress * 100)}%";
        }

        private IEnumerator Fade(float targetAlpha)
        {
            if (fadeOverlay == null) yield break;

            float startAlpha = fadeOverlay.alpha;
            float elapsed = 0f;

            while (elapsed < fadeTime)
            {
                elapsed += Time.unscaledDeltaTime;
                fadeOverlay.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / fadeTime);
                yield return null;
            }

            fadeOverlay.alpha = targetAlpha;
        }

        /// <summary>
        /// 메인 메뉴로 이동
        /// </summary>
        public void GoToMainMenu()
        {
            Time.timeScale = 1f;
            LoadScene("MainMenu");
        }

        /// <summary>
        /// 게임 씬으로 이동
        /// </summary>
        public void StartGame()
        {
            LoadScene("GameScene");
        }
    }
}
