using UnityEngine;
using System;
using System.IO;

namespace GoldenAge.Utilities
{
    /// <summary>
    /// 스크린샷 캡처 시스템
    /// </summary>
    public class ScreenshotSystem : MonoBehaviour
    {
        public static ScreenshotSystem Instance { get; private set; }

        [Header("설정")]
        [SerializeField] private KeyCode screenshotKey = KeyCode.F12;
        [SerializeField] private int superSize = 1; // 해상도 배율
        [SerializeField] private bool includeUI = true;
        [SerializeField] private string folderName = "Screenshots";

        [Header("효과")]
        [SerializeField] private bool flashEffect = true;
        [SerializeField] private AudioClip shutterSound;
        [SerializeField] private GameObject flashPanel;
        [SerializeField] private float flashDuration = 0.1f;

        private string savePath;
        private bool isCapturing = false;

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

            // 저장 경로 설정
            savePath = Path.Combine(Application.persistentDataPath, folderName);
            if (!Directory.Exists(savePath))
            {
                Directory.CreateDirectory(savePath);
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(screenshotKey))
            {
                CaptureScreenshot();
            }
        }

        /// <summary>
        /// 스크린샷 캡처
        /// </summary>
        public void CaptureScreenshot()
        {
            if (isCapturing) return;
            StartCoroutine(CaptureCoroutine());
        }

        private System.Collections.IEnumerator CaptureCoroutine()
        {
            isCapturing = true;

            // UI 숨기기 (선택적)
            Canvas[] canvases = null;
            if (!includeUI)
            {
                canvases = FindObjectsOfType<Canvas>();
                foreach (var canvas in canvases)
                {
                    canvas.enabled = false;
                }
            }

            // 프레임 끝까지 대기
            yield return new WaitForEndOfFrame();

            // 파일명 생성
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            string filename = $"GoldenAge_{timestamp}.png";
            string fullPath = Path.Combine(savePath, filename);

            // 캡처
            ScreenCapture.CaptureScreenshot(fullPath, superSize);

            // UI 복원
            if (!includeUI && canvases != null)
            {
                foreach (var canvas in canvases)
                {
                    canvas.enabled = true;
                }
            }

            // 효과
            if (flashEffect)
            {
                yield return StartCoroutine(FlashCoroutine());
            }

            if (shutterSound != null)
            {
                AudioSource.PlayClipAtPoint(shutterSound, Camera.main.transform.position);
            }

            // 알림
            UI.NotificationSystem.Instance?.ShowSuccess($"스크린샷 저장됨");

            Debug.Log($"[Screenshot] 저장됨: {fullPath}");

            isCapturing = false;
        }

        private System.Collections.IEnumerator FlashCoroutine()
        {
            if (flashPanel != null)
            {
                flashPanel.SetActive(true);
                yield return new WaitForSeconds(flashDuration);
                flashPanel.SetActive(false);
            }
        }

        /// <summary>
        /// 저장 폴더 열기
        /// </summary>
        public void OpenScreenshotFolder()
        {
            Application.OpenURL("file://" + savePath);
        }

        /// <summary>
        /// 저장 경로 가져오기
        /// </summary>
        public string GetSavePath()
        {
            return savePath;
        }
    }
}
