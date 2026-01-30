using UnityEngine;
using UnityEngine.SceneManagement;
using GoldenAge.Core;

namespace GoldenAge.UI
{
    /// <summary>
    /// 일시정지 메뉴
    /// </summary>
    public class PauseMenu : MonoBehaviour
    {
        [Header("UI 패널")]
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private GameObject settingsPanel;

        [Header("오디오")]
        [SerializeField] private AudioClip openSound;
        [SerializeField] private AudioClip closeSound;

        private bool isPaused = false;
        private PlayerInputActions inputActions;

        private void Start()
        {
            // 초기 상태: 숨김
            if (pausePanel != null)
                pausePanel.SetActive(false);
            if (settingsPanel != null)
                settingsPanel.SetActive(false);

            // 입력 설정
            inputActions = new PlayerInputActions();
            inputActions.Player.Pause.performed += ctx => TogglePause();
            inputActions.Player.Enable();
        }

        private void OnDestroy()
        {
            inputActions?.Player.Disable();
        }

        public void TogglePause()
        {
            if (isPaused)
                Resume();
            else
                Pause();
        }

        public void Pause()
        {
            isPaused = true;
            Time.timeScale = 0f;

            if (pausePanel != null)
                pausePanel.SetActive(true);

            // 커서 표시
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            // 게임 상태 변경
            GameManager.Instance?.SetState(GameState.Paused);

            // 사운드
            if (openSound != null)
                AudioSource.PlayClipAtPoint(openSound, Camera.main.transform.position);
        }

        public void Resume()
        {
            isPaused = false;
            Time.timeScale = 1f;

            if (pausePanel != null)
                pausePanel.SetActive(false);
            if (settingsPanel != null)
                settingsPanel.SetActive(false);

            // 커서 숨김
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            // 게임 상태 변경
            GameManager.Instance?.SetState(GameState.Playing);

            // 사운드
            if (closeSound != null)
                AudioSource.PlayClipAtPoint(closeSound, Camera.main.transform.position);
        }

        public void OpenSettings()
        {
            if (pausePanel != null)
                pausePanel.SetActive(false);
            if (settingsPanel != null)
                settingsPanel.SetActive(true);
        }

        public void CloseSettings()
        {
            if (settingsPanel != null)
                settingsPanel.SetActive(false);
            if (pausePanel != null)
                pausePanel.SetActive(true);
        }

        public void ReturnToMainMenu()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene("MainMenu");
        }

        public void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
