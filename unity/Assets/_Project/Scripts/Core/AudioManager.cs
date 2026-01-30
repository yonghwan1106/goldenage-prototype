using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace GoldenAge.Core
{
    /// <summary>
    /// 오디오(BGM, SFX, Voice) 재생을 관리하는 매니저
    /// </summary>
    public class AudioManager : Singleton<AudioManager>
    {
        [Header("Audio Sources")]
        [SerializeField] private AudioSource bgmSource;
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioSource voiceSource;

        [Header("Audio Clips")]
        [SerializeField] private List<AudioClipData> bgmClips = new List<AudioClipData>();
        [SerializeField] private List<AudioClipData> sfxClips = new List<AudioClipData>();

        [Header("Settings")]
        [SerializeField] private float bgmFadeDuration = 1f;
        [SerializeField] private float defaultBGMVolume = 0.5f;
        [SerializeField] private float defaultSFXVolume = 1f;
        [SerializeField] private float defaultVoiceVolume = 1f;

        private Dictionary<string, AudioClip> bgmDict = new Dictionary<string, AudioClip>();
        private Dictionary<string, AudioClip> sfxDict = new Dictionary<string, AudioClip>();
        private Coroutine fadeCoroutine;

        protected override void Awake()
        {
            base.Awake();
            InitializeAudioSources();
            BuildDictionaries();
        }

        private void InitializeAudioSources()
        {
            if (bgmSource == null)
            {
                bgmSource = gameObject.AddComponent<AudioSource>();
                bgmSource.loop = true;
                bgmSource.playOnAwake = false;
                bgmSource.volume = defaultBGMVolume;
            }

            if (sfxSource == null)
            {
                sfxSource = gameObject.AddComponent<AudioSource>();
                sfxSource.loop = false;
                sfxSource.playOnAwake = false;
                sfxSource.volume = defaultSFXVolume;
            }

            if (voiceSource == null)
            {
                voiceSource = gameObject.AddComponent<AudioSource>();
                voiceSource.loop = false;
                voiceSource.playOnAwake = false;
                voiceSource.volume = defaultVoiceVolume;
            }
        }

        private void BuildDictionaries()
        {
            foreach (var clip in bgmClips)
            {
                if (!string.IsNullOrEmpty(clip.name) && clip.clip != null)
                {
                    bgmDict[clip.name] = clip.clip;
                }
            }

            foreach (var clip in sfxClips)
            {
                if (!string.IsNullOrEmpty(clip.name) && clip.clip != null)
                {
                    sfxDict[clip.name] = clip.clip;
                }
            }
        }

        #region BGM

        /// <summary>
        /// BGM 재생 (이름으로)
        /// </summary>
        public void PlayBGM(string name)
        {
            if (bgmDict.TryGetValue(name, out AudioClip clip))
            {
                PlayBGM(clip);
            }
            else
            {
                Debug.LogWarning($"[AudioManager] BGM not found: {name}");
            }
        }

        /// <summary>
        /// BGM 재생 (클립으로)
        /// </summary>
        public void PlayBGM(AudioClip clip)
        {
            if (clip == null) return;

            if (bgmSource.clip == clip && bgmSource.isPlaying)
                return;

            if (fadeCoroutine != null)
                StopCoroutine(fadeCoroutine);

            fadeCoroutine = StartCoroutine(FadeBGM(clip));
        }

        private IEnumerator FadeBGM(AudioClip newClip)
        {
            float startVolume = bgmSource.volume;

            // 페이드 아웃
            if (bgmSource.isPlaying)
            {
                while (bgmSource.volume > 0)
                {
                    bgmSource.volume -= startVolume * Time.unscaledDeltaTime / (bgmFadeDuration / 2);
                    yield return null;
                }
            }

            // 클립 변경
            bgmSource.clip = newClip;
            bgmSource.Play();

            // 페이드 인
            while (bgmSource.volume < startVolume)
            {
                bgmSource.volume += startVolume * Time.unscaledDeltaTime / (bgmFadeDuration / 2);
                yield return null;
            }

            bgmSource.volume = startVolume;
        }

        /// <summary>
        /// BGM 정지
        /// </summary>
        public void StopBGM(bool fade = true)
        {
            if (fade)
            {
                StartCoroutine(FadeOutBGM());
            }
            else
            {
                bgmSource.Stop();
            }
        }

        private IEnumerator FadeOutBGM()
        {
            float startVolume = bgmSource.volume;

            while (bgmSource.volume > 0)
            {
                bgmSource.volume -= startVolume * Time.unscaledDeltaTime / bgmFadeDuration;
                yield return null;
            }

            bgmSource.Stop();
            bgmSource.volume = startVolume;
        }

        #endregion

        #region SFX

        /// <summary>
        /// SFX 재생 (이름으로)
        /// </summary>
        public void PlaySFX(string name)
        {
            if (sfxDict.TryGetValue(name, out AudioClip clip))
            {
                PlaySFX(clip);
            }
            else
            {
                Debug.LogWarning($"[AudioManager] SFX not found: {name}");
            }
        }

        /// <summary>
        /// SFX 재생 (클립으로)
        /// </summary>
        public void PlaySFX(AudioClip clip)
        {
            if (clip != null)
            {
                sfxSource.PlayOneShot(clip);
            }
        }

        /// <summary>
        /// 3D 위치에서 SFX 재생
        /// </summary>
        public void PlaySFXAtPosition(AudioClip clip, Vector3 position)
        {
            if (clip != null)
            {
                AudioSource.PlayClipAtPoint(clip, position, sfxSource.volume);
            }
        }

        #endregion

        #region Voice

        /// <summary>
        /// 음성 재생
        /// </summary>
        public void PlayVoice(AudioClip clip)
        {
            if (clip == null) return;

            voiceSource.Stop();
            voiceSource.clip = clip;
            voiceSource.Play();
        }

        /// <summary>
        /// 음성 정지
        /// </summary>
        public void StopVoice()
        {
            voiceSource.Stop();
        }

        /// <summary>
        /// 음성 재생 중인지 확인
        /// </summary>
        public bool IsVoicePlaying()
        {
            return voiceSource.isPlaying;
        }

        #endregion

        #region Volume Control

        public void SetBGMVolume(float volume)
        {
            bgmSource.volume = Mathf.Clamp01(volume);
        }

        public void SetSFXVolume(float volume)
        {
            sfxSource.volume = Mathf.Clamp01(volume);
        }

        public void SetVoiceVolume(float volume)
        {
            voiceSource.volume = Mathf.Clamp01(volume);
        }

        public void SetMasterVolume(float volume)
        {
            AudioListener.volume = Mathf.Clamp01(volume);
        }

        #endregion

        /// <summary>
        /// 모든 오디오 정지
        /// </summary>
        public void StopAll()
        {
            bgmSource.Stop();
            sfxSource.Stop();
            voiceSource.Stop();
        }
    }

    [System.Serializable]
    public class AudioClipData
    {
        public string name;
        public AudioClip clip;
    }
}
