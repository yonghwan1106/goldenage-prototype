using UnityEngine;

namespace GoldenAge.Core
{
    /// <summary>
    /// 게임 내 시간 및 조명 시스템
    /// </summary>
    public class TimeSystem : Singleton<TimeSystem>
    {
        [Header("시간 설정")]
        [SerializeField] private float gameTimeScale = 60f; // 1초 = 1분 (60배속)
        [SerializeField] private float startHour = 18f; // 시작 시간 (저녁 6시)
        [SerializeField] private bool pauseTimeInMenu = true;

        [Header("조명 참조")]
        [SerializeField] private Light sunLight;
        [SerializeField] private Light moonLight;

        [Header("하늘 설정")]
        [SerializeField] private Material skyboxMaterial;
        [SerializeField] private Gradient skyColorGradient;
        [SerializeField] private Gradient ambientColorGradient;
        [SerializeField] private AnimationCurve sunIntensityCurve;

        [Header("시간대별 색상")]
        [SerializeField] private Color dawnColor = new Color(1f, 0.7f, 0.5f);
        [SerializeField] private Color dayColor = new Color(1f, 0.95f, 0.85f);
        [SerializeField] private Color duskColor = new Color(1f, 0.5f, 0.3f);
        [SerializeField] private Color nightColor = new Color(0.3f, 0.35f, 0.5f);

        [Header("1920년대 분위기")]
        [SerializeField] private bool enableStreetLights = true;
        [SerializeField] private Light[] streetLights;
        [SerializeField] private float streetLightOnHour = 19f;
        [SerializeField] private float streetLightOffHour = 6f;

        // 현재 시간
        private float currentTimeInHours;
        private int currentDay = 1;

        // 상태
        private bool isNight = false;
        private TimePeriod currentPeriod = TimePeriod.Evening;

        // 프로퍼티
        public float CurrentHour => currentTimeInHours;
        public int CurrentDay => currentDay;
        public bool IsNight => isNight;
        public TimePeriod CurrentPeriod => currentPeriod;
        public string FormattedTime => FormatTime(currentTimeInHours);

        // 이벤트
        public event System.Action<TimePeriod> OnPeriodChanged;
        public event System.Action OnDayChanged;
        public event System.Action OnNightStart;
        public event System.Action OnDayStart;

        protected override void Awake()
        {
            base.Awake();
            currentTimeInHours = startHour;
            UpdateTimePeriod();
        }

        private void Update()
        {
            if (pauseTimeInMenu && GameManager.Instance?.CurrentState == GameState.Paused)
                return;

            // 시간 진행
            float deltaHours = (Time.deltaTime * gameTimeScale) / 3600f;
            currentTimeInHours += deltaHours;

            // 자정 넘김
            if (currentTimeInHours >= 24f)
            {
                currentTimeInHours -= 24f;
                currentDay++;
                OnDayChanged?.Invoke();
            }

            // 시간대 업데이트
            UpdateTimePeriod();

            // 조명 업데이트
            UpdateLighting();

            // 가로등
            if (enableStreetLights)
            {
                UpdateStreetLights();
            }
        }

        private void UpdateTimePeriod()
        {
            TimePeriod newPeriod;
            bool newIsNight;

            if (currentTimeInHours >= 5f && currentTimeInHours < 7f)
            {
                newPeriod = TimePeriod.Dawn;
                newIsNight = false;
            }
            else if (currentTimeInHours >= 7f && currentTimeInHours < 17f)
            {
                newPeriod = TimePeriod.Day;
                newIsNight = false;
            }
            else if (currentTimeInHours >= 17f && currentTimeInHours < 20f)
            {
                newPeriod = TimePeriod.Evening;
                newIsNight = false;
            }
            else
            {
                newPeriod = TimePeriod.Night;
                newIsNight = true;
            }

            // 시간대 변경 이벤트
            if (newPeriod != currentPeriod)
            {
                currentPeriod = newPeriod;
                OnPeriodChanged?.Invoke(currentPeriod);
            }

            // 밤/낮 변경 이벤트
            if (newIsNight != isNight)
            {
                isNight = newIsNight;
                if (isNight)
                    OnNightStart?.Invoke();
                else
                    OnDayStart?.Invoke();
            }
        }

        private void UpdateLighting()
        {
            float normalizedTime = currentTimeInHours / 24f;

            // 태양 위치 (6시 일출, 18시 일몰)
            float sunAngle = (currentTimeInHours - 6f) * 15f; // 시간당 15도
            if (sunLight != null)
            {
                sunLight.transform.rotation = Quaternion.Euler(sunAngle, -30f, 0f);

                // 태양 강도
                float intensity = sunIntensityCurve != null
                    ? sunIntensityCurve.Evaluate(normalizedTime)
                    : CalculateSunIntensity();

                sunLight.intensity = intensity;

                // 태양 색상
                sunLight.color = GetSunColor();

                // 낮에만 활성화
                sunLight.enabled = !isNight;
            }

            // 달 (밤에만)
            if (moonLight != null)
            {
                moonLight.enabled = isNight;
                if (isNight)
                {
                    float moonAngle = (currentTimeInHours - 18f) * 15f;
                    moonLight.transform.rotation = Quaternion.Euler(moonAngle, 150f, 0f);
                    moonLight.intensity = 0.3f;
                    moonLight.color = new Color(0.7f, 0.8f, 1f);
                }
            }

            // 앰비언트 라이트
            if (ambientColorGradient != null)
            {
                RenderSettings.ambientLight = ambientColorGradient.Evaluate(normalizedTime);
            }
            else
            {
                RenderSettings.ambientLight = GetAmbientColor();
            }

            // 스카이박스
            if (skyboxMaterial != null && skyColorGradient != null)
            {
                Color skyColor = skyColorGradient.Evaluate(normalizedTime);
                skyboxMaterial.SetColor("_SkyTint", skyColor);
            }
        }

        private float CalculateSunIntensity()
        {
            // 6시~18시 사이에 강도 증가/감소
            if (currentTimeInHours < 6f || currentTimeInHours > 18f)
                return 0f;

            float noon = 12f;
            float distanceFromNoon = Mathf.Abs(currentTimeInHours - noon);
            float maxDistance = 6f;

            return Mathf.Lerp(1f, 0.2f, distanceFromNoon / maxDistance);
        }

        private Color GetSunColor()
        {
            if (currentTimeInHours >= 5f && currentTimeInHours < 7f)
                return Color.Lerp(nightColor, dawnColor, (currentTimeInHours - 5f) / 2f);
            else if (currentTimeInHours >= 7f && currentTimeInHours < 10f)
                return Color.Lerp(dawnColor, dayColor, (currentTimeInHours - 7f) / 3f);
            else if (currentTimeInHours >= 10f && currentTimeInHours < 16f)
                return dayColor;
            else if (currentTimeInHours >= 16f && currentTimeInHours < 19f)
                return Color.Lerp(dayColor, duskColor, (currentTimeInHours - 16f) / 3f);
            else if (currentTimeInHours >= 19f && currentTimeInHours < 21f)
                return Color.Lerp(duskColor, nightColor, (currentTimeInHours - 19f) / 2f);
            else
                return nightColor;
        }

        private Color GetAmbientColor()
        {
            Color sunColor = GetSunColor();
            return sunColor * 0.4f;
        }

        private void UpdateStreetLights()
        {
            if (streetLights == null) return;

            bool shouldBeOn = (currentTimeInHours >= streetLightOnHour || currentTimeInHours < streetLightOffHour);

            foreach (var light in streetLights)
            {
                if (light != null)
                {
                    light.enabled = shouldBeOn;
                }
            }
        }

        /// <summary>
        /// 시간 설정
        /// </summary>
        public void SetTime(float hour)
        {
            currentTimeInHours = Mathf.Clamp(hour, 0f, 24f);
            UpdateTimePeriod();
            UpdateLighting();
        }

        /// <summary>
        /// 시간 건너뛰기
        /// </summary>
        public void SkipHours(float hours)
        {
            SetTime(currentTimeInHours + hours);
        }

        /// <summary>
        /// 시간 포맷팅
        /// </summary>
        public string FormatTime(float hours)
        {
            int h = Mathf.FloorToInt(hours) % 24;
            int m = Mathf.FloorToInt((hours % 1f) * 60f);
            return $"{h:D2}:{m:D2}";
        }

        /// <summary>
        /// 가로등 등록
        /// </summary>
        public void RegisterStreetLight(Light light)
        {
            var list = new System.Collections.Generic.List<Light>(streetLights ?? new Light[0]);
            if (!list.Contains(light))
            {
                list.Add(light);
                streetLights = list.ToArray();
            }
        }
    }

    public enum TimePeriod
    {
        Dawn,       // 새벽 (5-7시)
        Day,        // 낮 (7-17시)
        Evening,    // 저녁 (17-20시)
        Night       // 밤 (20-5시)
    }
}
