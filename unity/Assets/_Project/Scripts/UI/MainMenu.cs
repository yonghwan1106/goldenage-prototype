using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

namespace GoldenAge.UI
{
    /// <summary>
    /// 메인 메뉴 관리
    /// </summary>
    public class MainMenu : MonoBehaviour
    {
        [Header("버튼")]
        [SerializeField] private Button newGameButton;
        [SerializeField] private Button continueButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button creditsButton;
        [SerializeField] private Button quitButton;

        [Header("패널")]
        [SerializeField] private GameObject mainPanel;
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private GameObject creditsPanel;

        [Header("설정")]
        [SerializeField] private Slider masterVolumeSlider;
        [SerializeField] private Slider musicVolumeSlider;
        [SerializeField] private Slider sfxVolumeSlider;
        [SerializeField] private TMP_Dropdown qualityDropdown;
        [SerializeField] private Toggle fullscreenToggle;

        [Header("씬")]
        [SerializeField] private string gameSceneName = "GameScene";

        [Header("오디오")]
        [SerializeField] private AudioSource bgmSource;
        [SerializeField] private AudioClip menuMusic;

        private void Start()
        {
            // 커서 표시
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            // 초기 패널 상태
            ShowMainPanel();

            // 버튼 이벤트 연결
            SetupButtons();

            // 설정 UI 초기화
            InitializeSettings();

            // BGM 재생
            if (bgmSource != null && menuMusic != null)
            {
                bgmSource.clip = menuMusic;
                bgmSource.loop = true;
                bgmSource.Play();
            }

            // 계속하기 버튼 (저장 데이터 확인)
            if (continueButton != null)
            {
                bool hasSaveData = PlayerPrefs.HasKey("SaveData");
                continueButton.interactable = hasSaveData;
            }
        }

        private void SetupButtons()
        {
            if (newGameButton != null)
                newGameButton.onClick.AddListener(NewGame);
            if (continueButton != null)
                continueButton.onClick.AddListener(ContinueGame);
            if (settingsButton != null)
                settingsButton.onClick.AddListener(ShowSettings);
            if (creditsButton != null)
                creditsButton.onClick.AddListener(ShowCredits);
            if (quitButton != null)
                quitButton.onClick.AddListener(QuitGame);
        }

        private void InitializeSettings()
        {
            // 볼륨 슬라이더
            if (masterVolumeSlider != null)
            {
                masterVolumeSlider.value = PlayerPrefs.GetFloat("MasterVolume", 1f);
                masterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);
            }

            if (musicVolumeSlider != null)
            {
                musicVolumeSlider.value = PlayerPrefs.GetFloat("MusicVolume", 0.7f);
                musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
            }

            if (sfxVolumeSlider != null)
            {
                sfxVolumeSlider.value = PlayerPrefs.GetFloat("SFXVolume", 1f);
                sfxVolumeSlider.onValueChanged.AddListener(SetSFXVolume);
            }

            // 품질 설정
            if (qualityDropdown != null)
            {
                qualityDropdown.ClearOptions();
                qualityDropdown.AddOptions(new System.Collections.Generic.List<string>(QualitySettings.names));
                qualityDropdown.value = QualitySettings.GetQualityLevel();
                qualityDropdown.onValueChanged.AddListener(SetQuality);
            }

            // 전체화면
            if (fullscreenToggle != null)
            {
                fullscreenToggle.isOn = Screen.fullScreen;
                fullscreenToggle.onValueChanged.AddListener(SetFullscreen);
            }
        }

        public void NewGame()
        {
            // 저장 데이터 초기화 (선택적)
            // PlayerPrefs.DeleteAll();

            SceneManager.LoadScene(gameSceneName);
        }

        public void ContinueGame()
        {
            // 저장 데이터 로드 후 게임 시작
            SceneManager.LoadScene(gameSceneName);
        }

        public void ShowMainPanel()
        {
            if (mainPanel != null) mainPanel.SetActive(true);
            if (settingsPanel != null) settingsPanel.SetActive(false);
            if (creditsPanel != null) creditsPanel.SetActive(false);
        }

        public void ShowSettings()
        {
            if (mainPanel != null) mainPanel.SetActive(false);
            if (settingsPanel != null) settingsPanel.SetActive(true);
            if (creditsPanel != null) creditsPanel.SetActive(false);
        }

        public void ShowCredits()
        {
            if (mainPanel != null) mainPanel.SetActive(false);
            if (settingsPanel != null) settingsPanel.SetActive(false);
            if (creditsPanel != null) creditsPanel.SetActive(true);
        }

        public void BackToMain()
        {
            ShowMainPanel();
        }

        public void SetMasterVolume(float value)
        {
            AudioListener.volume = value;
            PlayerPrefs.SetFloat("MasterVolume", value);
        }

        public void SetMusicVolume(float value)
        {
            if (bgmSource != null)
                bgmSource.volume = value;
            PlayerPrefs.SetFloat("MusicVolume", value);
        }

        public void SetSFXVolume(float value)
        {
            PlayerPrefs.SetFloat("SFXVolume", value);
        }

        public void SetQuality(int qualityIndex)
        {
            QualitySettings.SetQualityLevel(qualityIndex);
            PlayerPrefs.SetInt("QualityLevel", qualityIndex);
        }

        public void SetFullscreen(bool isFullscreen)
        {
            Screen.fullScreen = isFullscreen;
            PlayerPrefs.SetInt("Fullscreen", isFullscreen ? 1 : 0);
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
