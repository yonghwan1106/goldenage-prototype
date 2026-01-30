using UnityEngine;
using UnityEngine.Audio;
using System;

namespace GoldenAge.Core
{
    /// <summary>
    /// 게임 설정 관리 시스템
    /// </summary>
    public class GameSettings : Singleton<GameSettings>
    {
        [Header("오디오 믹서")]
        [SerializeField] private AudioMixer audioMixer;

        // 설정 데이터
        private SettingsData settings;

        // 이벤트
        public event Action<SettingsData> OnSettingsChanged;

        public SettingsData Settings => settings;

        protected override void Awake()
        {
            base.Awake();
            LoadSettings();
        }

        private void Start()
        {
            ApplySettings();
        }

        /// <summary>
        /// 설정 불러오기
        /// </summary>
        public void LoadSettings()
        {
            string json = PlayerPrefs.GetString("GameSettings", "");

            if (string.IsNullOrEmpty(json))
            {
                settings = new SettingsData();
                Debug.Log("[GameSettings] 기본 설정 사용");
            }
            else
            {
                settings = JsonUtility.FromJson<SettingsData>(json);
                Debug.Log("[GameSettings] 저장된 설정 불러옴");
            }
        }

        /// <summary>
        /// 설정 저장
        /// </summary>
        public void SaveSettings()
        {
            string json = JsonUtility.ToJson(settings);
            PlayerPrefs.SetString("GameSettings", json);
            PlayerPrefs.Save();

            Debug.Log("[GameSettings] 설정 저장됨");
        }

        /// <summary>
        /// 설정 적용
        /// </summary>
        public void ApplySettings()
        {
            // 오디오
            ApplyAudioSettings();

            // 그래픽
            ApplyGraphicsSettings();

            // 이벤트
            OnSettingsChanged?.Invoke(settings);
        }

        private void ApplyAudioSettings()
        {
            // 마스터 볼륨
            AudioListener.volume = settings.masterVolume;

            // 믹서 사용 시
            if (audioMixer != null)
            {
                audioMixer.SetFloat("MasterVolume", VolumeToDecibel(settings.masterVolume));
                audioMixer.SetFloat("MusicVolume", VolumeToDecibel(settings.musicVolume));
                audioMixer.SetFloat("SFXVolume", VolumeToDecibel(settings.sfxVolume));
                audioMixer.SetFloat("VoiceVolume", VolumeToDecibel(settings.voiceVolume));
            }
        }

        private void ApplyGraphicsSettings()
        {
            // 품질
            QualitySettings.SetQualityLevel(settings.qualityLevel);

            // 해상도
            if (settings.resolutionIndex >= 0 && settings.resolutionIndex < Screen.resolutions.Length)
            {
                Resolution res = Screen.resolutions[settings.resolutionIndex];
                Screen.SetResolution(res.width, res.height, settings.fullscreen);
            }
            else
            {
                Screen.fullScreen = settings.fullscreen;
            }

            // VSync
            QualitySettings.vSyncCount = settings.vsync ? 1 : 0;

            // 프레임 제한
            Application.targetFrameRate = settings.targetFrameRate;
        }

        // 볼륨을 데시벨로 변환
        private float VolumeToDecibel(float volume)
        {
            return volume > 0 ? Mathf.Log10(volume) * 20f : -80f;
        }

        #region Setters

        public void SetMasterVolume(float value)
        {
            settings.masterVolume = Mathf.Clamp01(value);
            ApplyAudioSettings();
        }

        public void SetMusicVolume(float value)
        {
            settings.musicVolume = Mathf.Clamp01(value);
            ApplyAudioSettings();
        }

        public void SetSFXVolume(float value)
        {
            settings.sfxVolume = Mathf.Clamp01(value);
            ApplyAudioSettings();
        }

        public void SetVoiceVolume(float value)
        {
            settings.voiceVolume = Mathf.Clamp01(value);
            ApplyAudioSettings();
        }

        public void SetQuality(int level)
        {
            settings.qualityLevel = level;
            ApplyGraphicsSettings();
        }

        public void SetResolution(int index)
        {
            settings.resolutionIndex = index;
            ApplyGraphicsSettings();
        }

        public void SetFullscreen(bool fullscreen)
        {
            settings.fullscreen = fullscreen;
            ApplyGraphicsSettings();
        }

        public void SetVSync(bool vsync)
        {
            settings.vsync = vsync;
            ApplyGraphicsSettings();
        }

        public void SetTargetFrameRate(int fps)
        {
            settings.targetFrameRate = fps;
            ApplyGraphicsSettings();
        }

        public void SetMouseSensitivity(float value)
        {
            settings.mouseSensitivity = Mathf.Clamp(value, 0.1f, 5f);
        }

        public void SetInvertY(bool invert)
        {
            settings.invertY = invert;
        }

        public void SetSubtitles(bool enabled)
        {
            settings.subtitlesEnabled = enabled;
        }

        public void SetLanguage(string lang)
        {
            settings.language = lang;
        }

        #endregion

        /// <summary>
        /// 기본값으로 리셋
        /// </summary>
        public void ResetToDefaults()
        {
            settings = new SettingsData();
            ApplySettings();
            SaveSettings();
            Debug.Log("[GameSettings] 기본값으로 리셋됨");
        }

        /// <summary>
        /// 사용 가능한 해상도 목록
        /// </summary>
        public Resolution[] GetAvailableResolutions()
        {
            return Screen.resolutions;
        }

        /// <summary>
        /// 품질 레벨 이름 목록
        /// </summary>
        public string[] GetQualityNames()
        {
            return QualitySettings.names;
        }
    }

    [Serializable]
    public class SettingsData
    {
        // 오디오
        public float masterVolume = 1f;
        public float musicVolume = 0.7f;
        public float sfxVolume = 1f;
        public float voiceVolume = 1f;

        // 그래픽
        public int qualityLevel = 2; // High
        public int resolutionIndex = -1; // -1 = 현재
        public bool fullscreen = true;
        public bool vsync = true;
        public int targetFrameRate = 60;

        // 조작
        public float mouseSensitivity = 1f;
        public bool invertY = false;

        // 접근성
        public bool subtitlesEnabled = true;
        public string language = "ko";

        // 게임플레이
        public bool showDamageNumbers = true;
        public bool showMinimap = true;
        public bool cameraShake = true;
    }
}
