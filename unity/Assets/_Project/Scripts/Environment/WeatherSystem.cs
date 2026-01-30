using UnityEngine;
using System.Collections;

namespace GoldenAge.Environment
{
    /// <summary>
    /// 날씨 시스템 (1920년대 뉴욕 분위기)
    /// </summary>
    public class WeatherSystem : MonoBehaviour
    {
        public static WeatherSystem Instance { get; private set; }

        [Header("파티클 시스템")]
        [SerializeField] private ParticleSystem rainParticles;
        [SerializeField] private ParticleSystem snowParticles;
        [SerializeField] private ParticleSystem fogParticles;
        [SerializeField] private ParticleSystem dustParticles;

        [Header("설정")]
        [SerializeField] private WeatherType currentWeather = WeatherType.Clear;
        [SerializeField] private float transitionDuration = 5f;
        [SerializeField] private bool autoChangeWeather = false;
        [SerializeField] private float minWeatherDuration = 300f; // 5분
        [SerializeField] private float maxWeatherDuration = 900f; // 15분

        [Header("비 설정")]
        [SerializeField] private float lightRainIntensity = 500f;
        [SerializeField] private float heavyRainIntensity = 2000f;
        [SerializeField] private AudioClip rainAmbience;
        [SerializeField] private AudioClip thunderSound;

        [Header("눈 설정")]
        [SerializeField] private float snowIntensity = 300f;
        [SerializeField] private AudioClip windAmbience;

        [Header("안개 설정")]
        [SerializeField] private float fogDensity = 0.05f;
        [SerializeField] private Color fogColor = new Color(0.5f, 0.5f, 0.55f);

        [Header("조명 영향")]
        [SerializeField] private Light sunLight;
        [SerializeField] private float clearSunIntensity = 1f;
        [SerializeField] private float cloudySunIntensity = 0.5f;
        [SerializeField] private float rainySunIntensity = 0.3f;

        private AudioSource ambienceSource;
        private Coroutine weatherCoroutine;
        private Coroutine autoChangeCoroutine;

        public WeatherType CurrentWeather => currentWeather;
        public event System.Action<WeatherType> OnWeatherChanged;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            // 앰비언스 오디오 소스 생성
            ambienceSource = gameObject.AddComponent<AudioSource>();
            ambienceSource.loop = true;
            ambienceSource.spatialBlend = 0f;
            ambienceSource.volume = 0f;
        }

        private void Start()
        {
            // 초기 날씨 적용
            ApplyWeatherInstant(currentWeather);

            if (autoChangeWeather)
            {
                StartAutoWeatherChange();
            }
        }

        /// <summary>
        /// 날씨 변경 (전환 효과)
        /// </summary>
        public void SetWeather(WeatherType weather)
        {
            if (weather == currentWeather) return;

            if (weatherCoroutine != null)
            {
                StopCoroutine(weatherCoroutine);
            }

            weatherCoroutine = StartCoroutine(TransitionWeather(weather));
        }

        /// <summary>
        /// 날씨 즉시 적용
        /// </summary>
        public void ApplyWeatherInstant(WeatherType weather)
        {
            currentWeather = weather;
            StopAllParticles();

            switch (weather)
            {
                case WeatherType.Clear:
                    SetFog(false, 0f, Color.white);
                    SetSunIntensity(clearSunIntensity);
                    StopAmbience();
                    break;

                case WeatherType.Cloudy:
                    SetFog(true, 0.01f, new Color(0.7f, 0.7f, 0.75f));
                    SetSunIntensity(cloudySunIntensity);
                    StopAmbience();
                    break;

                case WeatherType.LightRain:
                    SetParticleEmission(rainParticles, lightRainIntensity);
                    SetFog(true, 0.02f, fogColor);
                    SetSunIntensity(rainySunIntensity);
                    PlayAmbience(rainAmbience, 0.3f);
                    break;

                case WeatherType.HeavyRain:
                    SetParticleEmission(rainParticles, heavyRainIntensity);
                    SetFog(true, 0.03f, fogColor);
                    SetSunIntensity(rainySunIntensity * 0.7f);
                    PlayAmbience(rainAmbience, 0.6f);
                    break;

                case WeatherType.Snow:
                    SetParticleEmission(snowParticles, snowIntensity);
                    SetFog(true, 0.02f, Color.white * 0.9f);
                    SetSunIntensity(cloudySunIntensity);
                    PlayAmbience(windAmbience, 0.2f);
                    break;

                case WeatherType.Fog:
                    SetParticleEmission(fogParticles, 100f);
                    SetFog(true, fogDensity, fogColor);
                    SetSunIntensity(cloudySunIntensity * 0.7f);
                    StopAmbience();
                    break;

                case WeatherType.Dust:
                    SetParticleEmission(dustParticles, 200f);
                    SetFog(true, 0.02f, new Color(0.6f, 0.55f, 0.45f));
                    SetSunIntensity(cloudySunIntensity);
                    PlayAmbience(windAmbience, 0.15f);
                    break;
            }

            OnWeatherChanged?.Invoke(weather);
        }

        private IEnumerator TransitionWeather(WeatherType targetWeather)
        {
            WeatherType startWeather = currentWeather;
            float elapsed = 0f;

            // 파티클 페이드 아웃
            yield return StartCoroutine(FadeOutCurrentWeather());

            // 새 날씨 적용
            currentWeather = targetWeather;

            // 파티클 페이드 인
            yield return StartCoroutine(FadeInNewWeather(targetWeather));

            OnWeatherChanged?.Invoke(targetWeather);
            Debug.Log($"[Weather] 날씨 변경됨: {targetWeather}");
        }

        private IEnumerator FadeOutCurrentWeather()
        {
            float elapsed = 0f;
            float duration = transitionDuration / 2f;

            float startVolume = ambienceSource.volume;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                ambienceSource.volume = Mathf.Lerp(startVolume, 0f, t);
                yield return null;
            }

            StopAllParticles();
        }

        private IEnumerator FadeInNewWeather(WeatherType weather)
        {
            float elapsed = 0f;
            float duration = transitionDuration / 2f;

            // 목표 값 설정
            float targetSunIntensity = clearSunIntensity;
            float targetFogDensity = 0f;
            Color targetFogColor = Color.white;
            float targetAmbientVolume = 0f;
            AudioClip targetAmbience = null;

            switch (weather)
            {
                case WeatherType.Clear:
                    targetSunIntensity = clearSunIntensity;
                    break;
                case WeatherType.Cloudy:
                    targetSunIntensity = cloudySunIntensity;
                    targetFogDensity = 0.01f;
                    break;
                case WeatherType.LightRain:
                    SetParticleEmission(rainParticles, lightRainIntensity);
                    targetSunIntensity = rainySunIntensity;
                    targetFogDensity = 0.02f;
                    targetFogColor = fogColor;
                    targetAmbience = rainAmbience;
                    targetAmbientVolume = 0.3f;
                    break;
                case WeatherType.HeavyRain:
                    SetParticleEmission(rainParticles, heavyRainIntensity);
                    targetSunIntensity = rainySunIntensity * 0.7f;
                    targetFogDensity = 0.03f;
                    targetFogColor = fogColor;
                    targetAmbience = rainAmbience;
                    targetAmbientVolume = 0.6f;
                    break;
                case WeatherType.Snow:
                    SetParticleEmission(snowParticles, snowIntensity);
                    targetSunIntensity = cloudySunIntensity;
                    targetFogDensity = 0.02f;
                    targetFogColor = Color.white * 0.9f;
                    targetAmbience = windAmbience;
                    targetAmbientVolume = 0.2f;
                    break;
                case WeatherType.Fog:
                    SetParticleEmission(fogParticles, 100f);
                    targetSunIntensity = cloudySunIntensity * 0.7f;
                    targetFogDensity = fogDensity;
                    targetFogColor = fogColor;
                    break;
                case WeatherType.Dust:
                    SetParticleEmission(dustParticles, 200f);
                    targetSunIntensity = cloudySunIntensity;
                    targetFogDensity = 0.02f;
                    targetFogColor = new Color(0.6f, 0.55f, 0.45f);
                    targetAmbience = windAmbience;
                    targetAmbientVolume = 0.15f;
                    break;
            }

            // 앰비언스 설정
            if (targetAmbience != null)
            {
                ambienceSource.clip = targetAmbience;
                ambienceSource.Play();
            }

            float startSunIntensity = sunLight != null ? sunLight.intensity : 1f;
            float startFogDensity = RenderSettings.fogDensity;
            Color startFogColor = RenderSettings.fogColor;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                // 조명
                if (sunLight != null)
                {
                    sunLight.intensity = Mathf.Lerp(startSunIntensity, targetSunIntensity, t);
                }

                // 안개
                RenderSettings.fogDensity = Mathf.Lerp(startFogDensity, targetFogDensity, t);
                RenderSettings.fogColor = Color.Lerp(startFogColor, targetFogColor, t);
                RenderSettings.fog = targetFogDensity > 0f;

                // 앰비언스 볼륨
                ambienceSource.volume = Mathf.Lerp(0f, targetAmbientVolume, t);

                yield return null;
            }
        }

        private void SetParticleEmission(ParticleSystem ps, float rate)
        {
            if (ps == null) return;

            var emission = ps.emission;
            emission.rateOverTime = rate;
            ps.Play();
        }

        private void StopAllParticles()
        {
            if (rainParticles != null) rainParticles.Stop();
            if (snowParticles != null) snowParticles.Stop();
            if (fogParticles != null) fogParticles.Stop();
            if (dustParticles != null) dustParticles.Stop();
        }

        private void SetFog(bool enabled, float density, Color color)
        {
            RenderSettings.fog = enabled;
            RenderSettings.fogDensity = density;
            RenderSettings.fogColor = color;
            RenderSettings.fogMode = FogMode.Exponential;
        }

        private void SetSunIntensity(float intensity)
        {
            if (sunLight != null)
            {
                sunLight.intensity = intensity;
            }
        }

        private void PlayAmbience(AudioClip clip, float volume)
        {
            if (clip == null) return;

            ambienceSource.clip = clip;
            ambienceSource.volume = volume;
            ambienceSource.Play();
        }

        private void StopAmbience()
        {
            ambienceSource.Stop();
            ambienceSource.volume = 0f;
        }

        /// <summary>
        /// 천둥 효과
        /// </summary>
        public void PlayThunder()
        {
            if (thunderSound != null)
            {
                AudioSource.PlayClipAtPoint(thunderSound, Camera.main.transform.position);
                // 화면 플래시 효과 (CameraShake와 연동 가능)
                Core.CameraShake.Instance?.Shake(0.3f, 0.1f);
            }
        }

        /// <summary>
        /// 자동 날씨 변경 시작
        /// </summary>
        public void StartAutoWeatherChange()
        {
            if (autoChangeCoroutine != null)
            {
                StopCoroutine(autoChangeCoroutine);
            }
            autoChangeCoroutine = StartCoroutine(AutoWeatherChangeCoroutine());
        }

        /// <summary>
        /// 자동 날씨 변경 중지
        /// </summary>
        public void StopAutoWeatherChange()
        {
            if (autoChangeCoroutine != null)
            {
                StopCoroutine(autoChangeCoroutine);
                autoChangeCoroutine = null;
            }
        }

        private IEnumerator AutoWeatherChangeCoroutine()
        {
            while (true)
            {
                float waitTime = UnityEngine.Random.Range(minWeatherDuration, maxWeatherDuration);
                yield return new WaitForSeconds(waitTime);

                // 랜덤 날씨 선택
                WeatherType[] weathers = (WeatherType[])System.Enum.GetValues(typeof(WeatherType));
                WeatherType newWeather = weathers[UnityEngine.Random.Range(0, weathers.Length)];

                SetWeather(newWeather);
            }
        }
    }

    public enum WeatherType
    {
        Clear,      // 맑음
        Cloudy,     // 흐림
        LightRain,  // 약한 비
        HeavyRain,  // 폭우
        Snow,       // 눈
        Fog,        // 안개
        Dust        // 먼지/황사
    }
}
