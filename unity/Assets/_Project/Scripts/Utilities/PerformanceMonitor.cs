using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Profiling;

namespace GoldenAge.Utilities
{
    /// <summary>
    /// 성능 모니터링 시스템
    /// </summary>
    public class PerformanceMonitor : MonoBehaviour
    {
        [Header("UI 참조")]
        [SerializeField] private GameObject monitorPanel;
        [SerializeField] private TextMeshProUGUI fpsText;
        [SerializeField] private TextMeshProUGUI memoryText;
        [SerializeField] private TextMeshProUGUI statsText;

        [Header("설정")]
        [SerializeField] private KeyCode toggleKey = KeyCode.F3;
        [SerializeField] private float updateInterval = 0.5f;
        [SerializeField] private bool showOnStart = false;

        [Header("FPS 색상")]
        [SerializeField] private Color goodFPSColor = Color.green;
        [SerializeField] private Color mediumFPSColor = Color.yellow;
        [SerializeField] private Color badFPSColor = Color.red;
        [SerializeField] private int goodFPSThreshold = 60;
        [SerializeField] private int badFPSThreshold = 30;

        private bool isVisible = false;
        private float updateTimer;

        // FPS 계산
        private int frameCount;
        private float deltaTimeSum;
        private float currentFPS;
        private float minFPS = float.MaxValue;
        private float maxFPS = 0f;

        // 메모리
        private long totalMemory;
        private long usedMemory;

        private void Start()
        {
            isVisible = showOnStart;
            UpdateVisibility();
        }

        private void Update()
        {
            // 토글
            if (Input.GetKeyDown(toggleKey))
            {
                ToggleDisplay();
            }

            if (!isVisible) return;

            // FPS 계산
            frameCount++;
            deltaTimeSum += Time.unscaledDeltaTime;

            // 업데이트 간격
            updateTimer += Time.unscaledDeltaTime;
            if (updateTimer >= updateInterval)
            {
                CalculateStats();
                UpdateUI();
                updateTimer = 0f;
            }
        }

        public void ToggleDisplay()
        {
            isVisible = !isVisible;
            UpdateVisibility();
        }

        private void UpdateVisibility()
        {
            if (monitorPanel != null)
                monitorPanel.SetActive(isVisible);
        }

        private void CalculateStats()
        {
            // FPS
            if (frameCount > 0 && deltaTimeSum > 0)
            {
                currentFPS = frameCount / deltaTimeSum;

                if (currentFPS < minFPS) minFPS = currentFPS;
                if (currentFPS > maxFPS) maxFPS = currentFPS;
            }

            frameCount = 0;
            deltaTimeSum = 0f;

            // 메모리
            totalMemory = Profiler.GetTotalReservedMemoryLong();
            usedMemory = Profiler.GetTotalAllocatedMemoryLong();
        }

        private void UpdateUI()
        {
            // FPS 텍스트
            if (fpsText != null)
            {
                Color fpsColor = currentFPS >= goodFPSThreshold ? goodFPSColor :
                                 currentFPS >= badFPSThreshold ? mediumFPSColor : badFPSColor;

                fpsText.text = $"FPS: {currentFPS:F0}";
                fpsText.color = fpsColor;
            }

            // 메모리 텍스트
            if (memoryText != null)
            {
                float usedMB = usedMemory / (1024f * 1024f);
                float totalMB = totalMemory / (1024f * 1024f);

                memoryText.text = $"Memory: {usedMB:F1} / {totalMB:F1} MB";
            }

            // 상세 통계
            if (statsText != null)
            {
                string stats = $"Min FPS: {minFPS:F0} | Max FPS: {maxFPS:F0}\n";
                stats += $"Delta Time: {Time.deltaTime * 1000:F1} ms\n";
                stats += $"Time Scale: {Time.timeScale:F1}x\n";
                stats += $"Scene Objects: {FindObjectsOfType<GameObject>().Length}";

                statsText.text = stats;
            }
        }

        /// <summary>
        /// 통계 리셋
        /// </summary>
        public void ResetStats()
        {
            minFPS = float.MaxValue;
            maxFPS = 0f;
        }

        /// <summary>
        /// 현재 FPS 가져오기
        /// </summary>
        public float GetCurrentFPS() => currentFPS;

        /// <summary>
        /// 평균 FPS 가져오기
        /// </summary>
        public float GetAverageFPS() => (minFPS + maxFPS) / 2f;
    }
}
