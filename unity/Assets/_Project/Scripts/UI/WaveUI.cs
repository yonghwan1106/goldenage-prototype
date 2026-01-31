using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GoldenAge.Combat;

namespace GoldenAge.UI
{
    /// <summary>
    /// 웨이브 정보 UI 표시
    /// </summary>
    public class WaveUI : MonoBehaviour
    {
        [Header("UI 요소")]
        [SerializeField] private TextMeshProUGUI waveText;
        [SerializeField] private TextMeshProUGUI enemyCountText;
        [SerializeField] private TextMeshProUGUI killCountText;
        [SerializeField] private Slider waveProgressBar;
        [SerializeField] private GameObject waveStartPanel;
        [SerializeField] private TextMeshProUGUI waveStartText;

        [Header("애니메이션")]
        [SerializeField] private float waveStartDisplayTime = 2f;
        [SerializeField] private Animator animator;

        private void Start()
        {
            if (WaveManager.Instance != null)
            {
                WaveManager.Instance.OnWaveStart += HandleWaveStart;
                WaveManager.Instance.OnWaveComplete += HandleWaveComplete;
                WaveManager.Instance.OnEnemyCountChanged += UpdateEnemyCount;
                WaveManager.Instance.OnEnemyKilled += UpdateKillCount;
                WaveManager.Instance.OnGameOver += HandleGameOver;
            }

            if (waveStartPanel != null)
                waveStartPanel.SetActive(false);

            UpdateUI();
        }

        private void OnDestroy()
        {
            if (WaveManager.Instance != null)
            {
                WaveManager.Instance.OnWaveStart -= HandleWaveStart;
                WaveManager.Instance.OnWaveComplete -= HandleWaveComplete;
                WaveManager.Instance.OnEnemyCountChanged -= UpdateEnemyCount;
                WaveManager.Instance.OnEnemyKilled -= UpdateKillCount;
                WaveManager.Instance.OnGameOver -= HandleGameOver;
            }
        }

        private void Update()
        {
            UpdateProgressBar();
        }

        private void HandleWaveStart(int waveNumber)
        {
            if (waveText != null)
                waveText.text = $"Wave {waveNumber}";

            // 웨이브 시작 알림 표시
            if (waveStartPanel != null && waveStartText != null)
            {
                waveStartText.text = $"WAVE {waveNumber}";
                waveStartPanel.SetActive(true);

                if (animator != null)
                    animator.SetTrigger("WaveStart");

                CancelInvoke(nameof(HideWaveStartPanel));
                Invoke(nameof(HideWaveStartPanel), waveStartDisplayTime);
            }

            UpdateUI();
        }

        private void HideWaveStartPanel()
        {
            if (waveStartPanel != null)
                waveStartPanel.SetActive(false);
        }

        private void HandleWaveComplete(int waveNumber, int killCount)
        {
            if (animator != null)
                animator.SetTrigger("WaveComplete");
        }

        private void UpdateEnemyCount(int current, int max)
        {
            if (enemyCountText != null)
                enemyCountText.text = $"Enemies: {current}";

            UpdateProgressBar();
        }

        private void UpdateKillCount(int totalKills)
        {
            if (killCountText != null)
                killCountText.text = $"Kills: {totalKills}";
        }

        private void UpdateProgressBar()
        {
            if (waveProgressBar != null && WaveManager.Instance != null)
            {
                waveProgressBar.value = WaveManager.Instance.WaveProgress;
            }
        }

        private void HandleGameOver()
        {
            if (waveStartPanel != null && waveStartText != null)
            {
                waveStartText.text = "GAME OVER";
                waveStartPanel.SetActive(true);
            }
        }

        private void UpdateUI()
        {
            if (WaveManager.Instance == null) return;

            if (waveText != null)
                waveText.text = $"Wave {WaveManager.Instance.CurrentWave}";

            if (enemyCountText != null)
                enemyCountText.text = $"Enemies: {WaveManager.Instance.EnemiesAlive}";

            if (killCountText != null)
                killCountText.text = $"Kills: {WaveManager.Instance.TotalEnemiesKilled}";
        }
    }
}
