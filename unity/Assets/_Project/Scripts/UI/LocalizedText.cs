using UnityEngine;
using TMPro;
using GoldenAge.Core;

namespace GoldenAge.UI
{
    /// <summary>
    /// UI 텍스트 자동 지역화 컴포넌트
    /// </summary>
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class LocalizedText : MonoBehaviour
    {
        [SerializeField] private string localizationKey;
        [SerializeField] private bool updateOnEnable = true;

        private TextMeshProUGUI textComponent;

        private void Awake()
        {
            textComponent = GetComponent<TextMeshProUGUI>();
        }

        private void OnEnable()
        {
            if (LocalizationSystem.Instance != null)
            {
                LocalizationSystem.Instance.OnLanguageChanged += OnLanguageChanged;
            }

            if (updateOnEnable)
            {
                UpdateText();
            }
        }

        private void OnDisable()
        {
            if (LocalizationSystem.Instance != null)
            {
                LocalizationSystem.Instance.OnLanguageChanged -= OnLanguageChanged;
            }
        }

        private void OnLanguageChanged(SystemLanguage language)
        {
            UpdateText();
        }

        /// <summary>
        /// 텍스트 업데이트
        /// </summary>
        public void UpdateText()
        {
            if (textComponent == null || string.IsNullOrEmpty(localizationKey))
                return;

            if (LocalizationSystem.Instance != null)
            {
                textComponent.text = LocalizationSystem.Instance.Get(localizationKey);
            }
        }

        /// <summary>
        /// 지역화 키 설정
        /// </summary>
        public void SetKey(string key)
        {
            localizationKey = key;
            UpdateText();
        }

        /// <summary>
        /// 포맷 문자열로 설정
        /// </summary>
        public void SetFormatted(string key, params object[] args)
        {
            if (textComponent == null) return;

            if (LocalizationSystem.Instance != null)
            {
                textComponent.text = LocalizationSystem.Instance.GetFormat(key, args);
            }
        }
    }
}
