using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GoldenAge.Player;
using GoldenAge.Combat;

namespace GoldenAge.UI
{
    /// <summary>
    /// 게임 HUD 관리
    /// </summary>
    public class GameHUD : MonoBehaviour
    {
        [Header("체력/에너지 바")]
        [SerializeField] private Slider healthBar;
        [SerializeField] private Slider energyBar;
        [SerializeField] private TextMeshProUGUI healthText;
        [SerializeField] private TextMeshProUGUI energyText;

        [Header("경험치/레벨")]
        [SerializeField] private Slider expBar;
        [SerializeField] private TextMeshProUGUI levelText;

        [Header("스킬 쿨다운")]
        [SerializeField] private Image skill1CooldownFill;
        [SerializeField] private Image skill2CooldownFill;
        [SerializeField] private TextMeshProUGUI skill1KeyText;
        [SerializeField] private TextMeshProUGUI skill2KeyText;

        [Header("상호작용 프롬프트")]
        [SerializeField] private GameObject interactionPrompt;
        [SerializeField] private TextMeshProUGUI interactionText;

        [Header("콤보 표시")]
        [SerializeField] private GameObject comboIndicator;
        [SerializeField] private Image comboTimerFill;
        [SerializeField] private TextMeshProUGUI comboText;

        [Header("미니맵")]
        [SerializeField] private RawImage minimapImage;

        [Header("크로스헤어")]
        [SerializeField] private GameObject crosshair;

        private PlayerStats playerStats;
        private PlayerCombat playerCombat;

        private void Start()
        {
            // 플레이어 참조 찾기
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerStats = player.GetComponent<PlayerStats>();
                playerCombat = player.GetComponent<PlayerCombat>();
            }

            // 초기 UI 설정
            if (interactionPrompt != null)
                interactionPrompt.SetActive(false);

            if (comboIndicator != null)
                comboIndicator.SetActive(false);

            // 스킬 키 텍스트
            if (skill1KeyText != null)
                skill1KeyText.text = "Q";
            if (skill2KeyText != null)
                skill2KeyText.text = "R";
        }

        private void Update()
        {
            UpdateHealthBar();
            UpdateEnergyBar();
            UpdateExpBar();
            UpdateSkillCooldowns();
            UpdateComboIndicator();
        }

        private void UpdateHealthBar()
        {
            if (playerStats == null || healthBar == null) return;

            float healthPercent = playerStats.CurrentHealth / playerStats.MaxHealth;
            healthBar.value = healthPercent;

            if (healthText != null)
                healthText.text = $"{Mathf.CeilToInt(playerStats.CurrentHealth)}/{Mathf.CeilToInt(playerStats.MaxHealth)}";
        }

        private void UpdateEnergyBar()
        {
            if (playerStats == null || energyBar == null) return;

            float energyPercent = playerStats.CurrentEnergy / playerStats.MaxEnergy;
            energyBar.value = energyPercent;

            if (energyText != null)
                energyText.text = $"{Mathf.CeilToInt(playerStats.CurrentEnergy)}/{Mathf.CeilToInt(playerStats.MaxEnergy)}";
        }

        private void UpdateExpBar()
        {
            if (playerStats == null) return;

            if (expBar != null)
            {
                float expPercent = (float)playerStats.Experience / playerStats.ExperienceToNextLevel;
                expBar.value = expPercent;
            }

            if (levelText != null)
                levelText.text = $"Lv.{playerStats.Level}";
        }

        private void UpdateSkillCooldowns()
        {
            if (playerCombat == null) return;

            // 스킬1 (테슬라) 쿨다운
            if (skill1CooldownFill != null)
            {
                float cooldown1 = playerCombat.GetSkillCooldownPercent(1);
                skill1CooldownFill.fillAmount = cooldown1;
            }

            // 스킬2 (에테르) 쿨다운
            if (skill2CooldownFill != null)
            {
                float cooldown2 = playerCombat.GetSkillCooldownPercent(2);
                skill2CooldownFill.fillAmount = cooldown2;
            }
        }

        private void UpdateComboIndicator()
        {
            if (playerCombat == null || comboIndicator == null) return;

            bool comboReady = playerCombat.IsFusionComboReady;
            comboIndicator.SetActive(comboReady);

            if (comboReady && comboTimerFill != null)
            {
                comboTimerFill.fillAmount = playerCombat.GetComboTimerPercent();
            }

            if (comboReady && comboText != null)
            {
                comboText.text = "차원 전격 준비!";
            }
        }

        /// <summary>
        /// 상호작용 프롬프트 표시
        /// </summary>
        public void ShowInteractionPrompt(string text)
        {
            if (interactionPrompt == null) return;

            interactionPrompt.SetActive(true);
            if (interactionText != null)
                interactionText.text = $"[E] {text}";
        }

        /// <summary>
        /// 상호작용 프롬프트 숨기기
        /// </summary>
        public void HideInteractionPrompt()
        {
            if (interactionPrompt != null)
                interactionPrompt.SetActive(false);
        }

        /// <summary>
        /// 데미지 플래시 효과
        /// </summary>
        public void ShowDamageFlash()
        {
            // 화면 가장자리 빨간색 플래시 효과
            // 구현 필요시 Image 컴포넌트 사용
        }

        /// <summary>
        /// 레벨업 알림
        /// </summary>
        public void ShowLevelUpNotification(int newLevel)
        {
            Debug.Log($"[HUD] 레벨 업! Lv.{newLevel}");
            // 레벨업 UI 팝업 표시
        }
    }
}
