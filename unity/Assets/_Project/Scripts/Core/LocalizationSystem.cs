using UnityEngine;
using System;
using System.Collections.Generic;

namespace GoldenAge.Core
{
    /// <summary>
    /// 다국어 지원 시스템
    /// </summary>
    public class LocalizationSystem : Singleton<LocalizationSystem>
    {
        [Header("설정")]
        [SerializeField] private SystemLanguage defaultLanguage = SystemLanguage.Korean;
        [SerializeField] private LocalizationData[] languageData;

        private Dictionary<string, string> currentStrings = new Dictionary<string, string>();
        private SystemLanguage currentLanguage;

        public SystemLanguage CurrentLanguage => currentLanguage;
        public event Action<SystemLanguage> OnLanguageChanged;

        // 지원 언어 목록
        private static readonly SystemLanguage[] SupportedLanguages = new SystemLanguage[]
        {
            SystemLanguage.Korean,
            SystemLanguage.English,
            SystemLanguage.Japanese,
            SystemLanguage.ChineseSimplified
        };

        protected override void Awake()
        {
            base.Awake();

            // 저장된 언어 설정 로드
            string savedLang = PlayerPrefs.GetString("Language", "");
            if (!string.IsNullOrEmpty(savedLang) && Enum.TryParse(savedLang, out SystemLanguage lang))
            {
                SetLanguage(lang);
            }
            else
            {
                // 시스템 언어 감지
                SystemLanguage systemLang = Application.systemLanguage;
                if (IsLanguageSupported(systemLang))
                {
                    SetLanguage(systemLang);
                }
                else
                {
                    SetLanguage(defaultLanguage);
                }
            }
        }

        /// <summary>
        /// 언어 설정
        /// </summary>
        public void SetLanguage(SystemLanguage language)
        {
            if (!IsLanguageSupported(language))
            {
                Debug.LogWarning($"[Localization] 지원하지 않는 언어: {language}");
                language = defaultLanguage;
            }

            currentLanguage = language;
            LoadLanguageData(language);

            // 저장
            PlayerPrefs.SetString("Language", language.ToString());
            PlayerPrefs.Save();

            OnLanguageChanged?.Invoke(language);
            Debug.Log($"[Localization] 언어 변경됨: {language}");
        }

        private void LoadLanguageData(SystemLanguage language)
        {
            currentStrings.Clear();

            if (languageData == null) return;

            foreach (var data in languageData)
            {
                if (data != null && data.language == language)
                {
                    foreach (var entry in data.entries)
                    {
                        if (!string.IsNullOrEmpty(entry.key))
                        {
                            currentStrings[entry.key] = entry.value;
                        }
                    }
                    break;
                }
            }
        }

        /// <summary>
        /// 번역된 텍스트 가져오기
        /// </summary>
        public string Get(string key)
        {
            if (string.IsNullOrEmpty(key)) return "";

            if (currentStrings.TryGetValue(key, out string value))
            {
                return value;
            }

            Debug.LogWarning($"[Localization] 키를 찾을 수 없음: {key}");
            return key; // 키를 그대로 반환
        }

        /// <summary>
        /// 포맷 문자열로 번역
        /// </summary>
        public string GetFormat(string key, params object[] args)
        {
            string format = Get(key);
            try
            {
                return string.Format(format, args);
            }
            catch
            {
                return format;
            }
        }

        /// <summary>
        /// 언어 지원 여부 확인
        /// </summary>
        public bool IsLanguageSupported(SystemLanguage language)
        {
            foreach (var lang in SupportedLanguages)
            {
                if (lang == language) return true;
            }
            return false;
        }

        /// <summary>
        /// 지원 언어 목록
        /// </summary>
        public SystemLanguage[] GetSupportedLanguages()
        {
            return SupportedLanguages;
        }

        /// <summary>
        /// 언어 이름 (해당 언어로)
        /// </summary>
        public string GetLanguageName(SystemLanguage language)
        {
            switch (language)
            {
                case SystemLanguage.Korean: return "한국어";
                case SystemLanguage.English: return "English";
                case SystemLanguage.Japanese: return "日本語";
                case SystemLanguage.ChineseSimplified: return "简体中文";
                default: return language.ToString();
            }
        }
    }

    [CreateAssetMenu(fileName = "LocalizationData", menuName = "GoldenAge/Localization Data")]
    public class LocalizationData : ScriptableObject
    {
        public SystemLanguage language;
        public List<LocalizationEntry> entries = new List<LocalizationEntry>();
    }

    [Serializable]
    public class LocalizationEntry
    {
        public string key;
        [TextArea] public string value;
    }
}
