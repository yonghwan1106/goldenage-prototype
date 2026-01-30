using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

namespace GoldenAge.UI
{
    /// <summary>
    /// 화면 알림/토스트 시스템
    /// </summary>
    public class NotificationSystem : MonoBehaviour
    {
        public static NotificationSystem Instance { get; private set; }

        [Header("UI 참조")]
        [SerializeField] private Transform notificationContainer;
        [SerializeField] private GameObject notificationPrefab;

        [Header("설정")]
        [SerializeField] private int maxNotifications = 5;
        [SerializeField] private float defaultDuration = 3f;
        [SerializeField] private float fadeInTime = 0.3f;
        [SerializeField] private float fadeOutTime = 0.5f;
        [SerializeField] private float slideDistance = 50f;

        [Header("타입별 색상")]
        [SerializeField] private Color infoColor = Color.white;
        [SerializeField] private Color successColor = new Color(0.3f, 0.9f, 0.4f);
        [SerializeField] private Color warningColor = new Color(1f, 0.8f, 0.2f);
        [SerializeField] private Color errorColor = new Color(1f, 0.3f, 0.3f);
        [SerializeField] private Color questColor = new Color(0.4f, 0.7f, 1f);
        [SerializeField] private Color itemColor = new Color(1f, 0.9f, 0.5f);

        [Header("아이콘")]
        [SerializeField] private Sprite infoIcon;
        [SerializeField] private Sprite successIcon;
        [SerializeField] private Sprite warningIcon;
        [SerializeField] private Sprite errorIcon;
        [SerializeField] private Sprite questIcon;
        [SerializeField] private Sprite itemIcon;

        private Queue<NotificationData> pendingNotifications = new Queue<NotificationData>();
        private List<NotificationItem> activeNotifications = new List<NotificationItem>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Update()
        {
            // 대기 중인 알림 처리
            while (pendingNotifications.Count > 0 && activeNotifications.Count < maxNotifications)
            {
                var data = pendingNotifications.Dequeue();
                ShowNotificationImmediate(data);
            }
        }

        /// <summary>
        /// 알림 표시
        /// </summary>
        public void Show(string message, NotificationType type = NotificationType.Info, float duration = 0)
        {
            if (duration <= 0) duration = defaultDuration;

            var data = new NotificationData
            {
                message = message,
                type = type,
                duration = duration
            };

            if (activeNotifications.Count >= maxNotifications)
            {
                pendingNotifications.Enqueue(data);
            }
            else
            {
                ShowNotificationImmediate(data);
            }
        }

        /// <summary>
        /// 정보 알림
        /// </summary>
        public void ShowInfo(string message)
        {
            Show(message, NotificationType.Info);
        }

        /// <summary>
        /// 성공 알림
        /// </summary>
        public void ShowSuccess(string message)
        {
            Show(message, NotificationType.Success);
        }

        /// <summary>
        /// 경고 알림
        /// </summary>
        public void ShowWarning(string message)
        {
            Show(message, NotificationType.Warning);
        }

        /// <summary>
        /// 에러 알림
        /// </summary>
        public void ShowError(string message)
        {
            Show(message, NotificationType.Error);
        }

        /// <summary>
        /// 퀘스트 알림
        /// </summary>
        public void ShowQuest(string message)
        {
            Show(message, NotificationType.Quest, 4f);
        }

        /// <summary>
        /// 아이템 획득 알림
        /// </summary>
        public void ShowItemPickup(string itemName, int amount = 1)
        {
            string msg = amount > 1 ? $"{itemName} x{amount} 획득" : $"{itemName} 획득";
            Show(msg, NotificationType.Item);
        }

        private void ShowNotificationImmediate(NotificationData data)
        {
            if (notificationPrefab == null || notificationContainer == null)
            {
                Debug.Log($"[Notification] {data.type}: {data.message}");
                return;
            }

            GameObject obj = Instantiate(notificationPrefab, notificationContainer);
            NotificationItem item = obj.GetComponent<NotificationItem>();

            if (item == null)
            {
                item = obj.AddComponent<NotificationItem>();
            }

            Color color = GetColorForType(data.type);
            Sprite icon = GetIconForType(data.type);

            item.Initialize(data.message, color, icon, data.duration, fadeInTime, fadeOutTime, slideDistance);
            item.OnComplete += () => OnNotificationComplete(item);

            activeNotifications.Add(item);
        }

        private void OnNotificationComplete(NotificationItem item)
        {
            activeNotifications.Remove(item);
            Destroy(item.gameObject);
        }

        private Color GetColorForType(NotificationType type)
        {
            return type switch
            {
                NotificationType.Success => successColor,
                NotificationType.Warning => warningColor,
                NotificationType.Error => errorColor,
                NotificationType.Quest => questColor,
                NotificationType.Item => itemColor,
                _ => infoColor
            };
        }

        private Sprite GetIconForType(NotificationType type)
        {
            return type switch
            {
                NotificationType.Success => successIcon,
                NotificationType.Warning => warningIcon,
                NotificationType.Error => errorIcon,
                NotificationType.Quest => questIcon,
                NotificationType.Item => itemIcon,
                _ => infoIcon
            };
        }

        /// <summary>
        /// 모든 알림 지우기
        /// </summary>
        public void ClearAll()
        {
            pendingNotifications.Clear();

            foreach (var item in activeNotifications)
            {
                if (item != null)
                {
                    Destroy(item.gameObject);
                }
            }
            activeNotifications.Clear();
        }
    }

    public enum NotificationType
    {
        Info,
        Success,
        Warning,
        Error,
        Quest,
        Item
    }

    public class NotificationData
    {
        public string message;
        public NotificationType type;
        public float duration;
    }

    /// <summary>
    /// 개별 알림 아이템
    /// </summary>
    public class NotificationItem : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI messageText;
        [SerializeField] private Image iconImage;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private CanvasGroup canvasGroup;

        private RectTransform rectTransform;
        private float duration;
        private float fadeInTime;
        private float fadeOutTime;
        private float slideDistance;

        public event System.Action OnComplete;

        public void Initialize(string message, Color color, Sprite icon, float duration, float fadeIn, float fadeOut, float slide)
        {
            rectTransform = GetComponent<RectTransform>();
            if (canvasGroup == null)
                canvasGroup = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();

            this.duration = duration;
            this.fadeInTime = fadeIn;
            this.fadeOutTime = fadeOut;
            this.slideDistance = slide;

            // UI 설정
            if (messageText != null)
                messageText.text = message;

            if (iconImage != null)
            {
                if (icon != null)
                {
                    iconImage.sprite = icon;
                    iconImage.gameObject.SetActive(true);
                }
                else
                {
                    iconImage.gameObject.SetActive(false);
                }
            }

            if (backgroundImage != null)
            {
                Color bgColor = color;
                bgColor.a = 0.9f;
                backgroundImage.color = bgColor;
            }

            // 애니메이션 시작
            StartCoroutine(AnimateNotification());
        }

        private IEnumerator AnimateNotification()
        {
            // 초기 상태
            canvasGroup.alpha = 0f;
            Vector2 startPos = rectTransform.anchoredPosition;
            startPos.x -= slideDistance;

            // 페이드 인 + 슬라이드
            float elapsed = 0f;
            while (elapsed < fadeInTime)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / fadeInTime;
                t = EaseOutCubic(t);

                canvasGroup.alpha = t;
                rectTransform.anchoredPosition = Vector2.Lerp(startPos, startPos + new Vector2(slideDistance, 0), t);

                yield return null;
            }

            canvasGroup.alpha = 1f;

            // 대기
            yield return new WaitForSecondsRealtime(duration);

            // 페이드 아웃
            elapsed = 0f;
            Vector2 endPos = rectTransform.anchoredPosition;

            while (elapsed < fadeOutTime)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / fadeOutTime;

                canvasGroup.alpha = 1f - t;

                yield return null;
            }

            OnComplete?.Invoke();
        }

        private float EaseOutCubic(float t)
        {
            return 1f - Mathf.Pow(1f - t, 3f);
        }
    }
}
