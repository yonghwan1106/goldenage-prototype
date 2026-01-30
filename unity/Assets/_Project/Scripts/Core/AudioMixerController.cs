using UnityEngine;
using UnityEngine.Audio;

namespace GoldenAge.Core
{
    /// <summary>
    /// 오디오 믹서 제어 시스템
    /// </summary>
    public class AudioMixerController : Singleton<AudioMixerController>
    {
        [Header("믹서")]
        [SerializeField] private AudioMixer masterMixer;

        [Header("파라미터 이름")]
        [SerializeField] private string masterVolumeParam = "MasterVolume";
        [SerializeField] private string bgmVolumeParam = "BGMVolume";
        [SerializeField] private string sfxVolumeParam = "SFXVolume";
        [SerializeField] private string voiceVolumeParam = "VoiceVolume";
        [SerializeField] private string ambienceVolumeParam = "AmbienceVolume";

        [Header("스냅샷")]
        [SerializeField] private AudioMixerSnapshot normalSnapshot;
        [SerializeField] private AudioMixerSnapshot pausedSnapshot;
        [SerializeField] private AudioMixerSnapshot cutsceneSnapshot;
        [SerializeField] private AudioMixerSnapshot combatSnapshot;

        private const float MIN_DB = -80f;
        private const float MAX_DB = 0f;

        protected override void Awake()
        {
            base.Awake();
            LoadVolumeSettings();
        }

        private void Start()
        {
            // 게임 상태에 따라 스냅샷 변경
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnStateChanged += OnGameStateChanged;
            }
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnStateChanged -= OnGameStateChanged;
            }
        }

        private void OnGameStateChanged(GameState newState)
        {
            switch (newState)
            {
                case GameState.Paused:
                    TransitionToSnapshot(pausedSnapshot, 0.5f);
                    break;
                case GameState.Cutscene:
                    TransitionToSnapshot(cutsceneSnapshot, 1f);
                    break;
                case GameState.Combat:
                    TransitionToSnapshot(combatSnapshot, 0.5f);
                    break;
                default:
                    TransitionToSnapshot(normalSnapshot, 0.5f);
                    break;
            }
        }

        /// <summary>
        /// 마스터 볼륨 설정 (0-1)
        /// </summary>
        public void SetMasterVolume(float volume)
        {
            SetVolume(masterVolumeParam, volume);
            PlayerPrefs.SetFloat("MasterVolume", volume);
        }

        /// <summary>
        /// BGM 볼륨 설정 (0-1)
        /// </summary>
        public void SetBGMVolume(float volume)
        {
            SetVolume(bgmVolumeParam, volume);
            PlayerPrefs.SetFloat("BGMVolume", volume);
        }

        /// <summary>
        /// 효과음 볼륨 설정 (0-1)
        /// </summary>
        public void SetSFXVolume(float volume)
        {
            SetVolume(sfxVolumeParam, volume);
            PlayerPrefs.SetFloat("SFXVolume", volume);
        }

        /// <summary>
        /// 보이스 볼륨 설정 (0-1)
        /// </summary>
        public void SetVoiceVolume(float volume)
        {
            SetVolume(voiceVolumeParam, volume);
            PlayerPrefs.SetFloat("VoiceVolume", volume);
        }

        /// <summary>
        /// 앰비언스 볼륨 설정 (0-1)
        /// </summary>
        public void SetAmbienceVolume(float volume)
        {
            SetVolume(ambienceVolumeParam, volume);
            PlayerPrefs.SetFloat("AmbienceVolume", volume);
        }

        private void SetVolume(string paramName, float normalizedVolume)
        {
            if (masterMixer == null) return;

            // 0-1을 dB로 변환 (로그 스케일)
            float db = normalizedVolume > 0.001f
                ? Mathf.Log10(normalizedVolume) * 20f
                : MIN_DB;

            db = Mathf.Clamp(db, MIN_DB, MAX_DB);
            masterMixer.SetFloat(paramName, db);
        }

        /// <summary>
        /// 볼륨 가져오기 (0-1)
        /// </summary>
        public float GetVolume(string paramName)
        {
            if (masterMixer == null) return 1f;

            if (masterMixer.GetFloat(paramName, out float db))
            {
                return Mathf.Pow(10f, db / 20f);
            }
            return 1f;
        }

        public float GetMasterVolume() => GetVolume(masterVolumeParam);
        public float GetBGMVolume() => GetVolume(bgmVolumeParam);
        public float GetSFXVolume() => GetVolume(sfxVolumeParam);
        public float GetVoiceVolume() => GetVolume(voiceVolumeParam);
        public float GetAmbienceVolume() => GetVolume(ambienceVolumeParam);

        /// <summary>
        /// 스냅샷 전환
        /// </summary>
        public void TransitionToSnapshot(AudioMixerSnapshot snapshot, float duration)
        {
            if (snapshot != null)
            {
                snapshot.TransitionTo(duration);
            }
        }

        /// <summary>
        /// 저장된 설정 로드
        /// </summary>
        private void LoadVolumeSettings()
        {
            SetMasterVolume(PlayerPrefs.GetFloat("MasterVolume", 1f));
            SetBGMVolume(PlayerPrefs.GetFloat("BGMVolume", 0.8f));
            SetSFXVolume(PlayerPrefs.GetFloat("SFXVolume", 1f));
            SetVoiceVolume(PlayerPrefs.GetFloat("VoiceVolume", 1f));
            SetAmbienceVolume(PlayerPrefs.GetFloat("AmbienceVolume", 0.6f));
        }

        /// <summary>
        /// 모든 오디오 일시 정지
        /// </summary>
        public void PauseAll()
        {
            AudioListener.pause = true;
        }

        /// <summary>
        /// 모든 오디오 재개
        /// </summary>
        public void ResumeAll()
        {
            AudioListener.pause = false;
        }

        /// <summary>
        /// 저주파 필터 효과 (수중, 멀리서 등)
        /// </summary>
        public void SetLowPassFilter(bool enabled, float cutoffFrequency = 1000f)
        {
            if (masterMixer == null) return;

            masterMixer.SetFloat("LowPassCutoff", enabled ? cutoffFrequency : 22000f);
        }

        /// <summary>
        /// 피치 조절 (슬로우모션 등)
        /// </summary>
        public void SetPitch(float pitch)
        {
            if (masterMixer == null) return;

            masterMixer.SetFloat("MasterPitch", Mathf.Clamp(pitch, 0.5f, 2f));
        }
    }
}
