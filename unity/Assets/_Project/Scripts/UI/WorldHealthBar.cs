using UnityEngine;
using UnityEngine.UI;

namespace GoldenAge.UI
{
    /// <summary>
    /// 월드 공간 체력바 (적/NPC 머리 위)
    /// </summary>
    public class WorldHealthBar : MonoBehaviour
    {
        [Header("UI 요소")]
        [SerializeField] private Slider healthSlider;
        [SerializeField] private Image fillImage;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("색상")]
        [SerializeField] private Color healthyColor = Color.green;
        [SerializeField] private Color damagedColor = Color.yellow;
        [SerializeField] private Color criticalColor = Color.red;
        [SerializeField] private float damagedThreshold = 0.5f;
        [SerializeField] private float criticalThreshold = 0.25f;

        [Header("설정")]
        [SerializeField] private Vector3 offset = new Vector3(0, 2.2f, 0);
        [SerializeField] private bool faceCamera = true;
        [SerializeField] private bool hideWhenFull = true;
        [SerializeField] private float hideDelay = 2f;
        [SerializeField] private float fadeSpeed = 3f;

        [Header("스케일")]
        [SerializeField] private bool scaleWithDistance = true;
        [SerializeField] private float minScale = 0.5f;
        [SerializeField] private float maxScale = 1.5f;
        [SerializeField] private float scaleDistance = 20f;

        private Transform target;
        private Camera mainCamera;
        private float currentHealth = 1f;
        private float maxHealth = 1f;
        private float hideTimer;
        private float targetAlpha = 1f;

        private void Start()
        {
            mainCamera = Camera.main;

            if (canvasGroup == null)
                canvasGroup = GetComponent<CanvasGroup>();

            // 초기 상태
            if (hideWhenFull)
            {
                targetAlpha = 0f;
                if (canvasGroup != null)
                    canvasGroup.alpha = 0f;
            }
        }

        private void LateUpdate()
        {
            // 타겟 따라가기
            if (target != null)
            {
                transform.position = target.position + offset;
            }

            // 카메라 향하기
            if (faceCamera && mainCamera != null)
            {
                transform.LookAt(mainCamera.transform);
                transform.Rotate(0, 180, 0);
            }

            // 거리에 따른 스케일
            if (scaleWithDistance && mainCamera != null)
            {
                float distance = Vector3.Distance(transform.position, mainCamera.transform.position);
                float scale = Mathf.Lerp(maxScale, minScale, distance / scaleDistance);
                scale = Mathf.Clamp(scale, minScale, maxScale);
                transform.localScale = Vector3.one * scale;
            }

            // 숨기기 타이머
            if (hideWhenFull && currentHealth >= maxHealth)
            {
                hideTimer += Time.deltaTime;
                if (hideTimer >= hideDelay)
                {
                    targetAlpha = 0f;
                }
            }

            // 페이드
            if (canvasGroup != null)
            {
                canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, targetAlpha, fadeSpeed * Time.deltaTime);
            }
        }

        /// <summary>
        /// 초기화
        /// </summary>
        public void Initialize(Transform target, float maxHealth)
        {
            this.target = target;
            this.maxHealth = maxHealth;
            this.currentHealth = maxHealth;

            UpdateHealthBar();
        }

        /// <summary>
        /// 체력 업데이트
        /// </summary>
        public void SetHealth(float health)
        {
            currentHealth = Mathf.Clamp(health, 0, maxHealth);
            hideTimer = 0f;
            targetAlpha = 1f;

            UpdateHealthBar();
        }

        /// <summary>
        /// 최대 체력 변경
        /// </summary>
        public void SetMaxHealth(float max)
        {
            maxHealth = max;
            currentHealth = Mathf.Min(currentHealth, maxHealth);
            UpdateHealthBar();
        }

        private void UpdateHealthBar()
        {
            float healthPercent = maxHealth > 0 ? currentHealth / maxHealth : 0f;

            // 슬라이더 값
            if (healthSlider != null)
            {
                healthSlider.value = healthPercent;
            }

            // 색상 변경
            if (fillImage != null)
            {
                if (healthPercent <= criticalThreshold)
                {
                    fillImage.color = criticalColor;
                }
                else if (healthPercent <= damagedThreshold)
                {
                    fillImage.color = damagedColor;
                }
                else
                {
                    fillImage.color = healthyColor;
                }
            }
        }

        /// <summary>
        /// 강제 표시
        /// </summary>
        public void Show()
        {
            hideTimer = 0f;
            targetAlpha = 1f;
        }

        /// <summary>
        /// 강제 숨기기
        /// </summary>
        public void Hide()
        {
            targetAlpha = 0f;
        }
    }

    /// <summary>
    /// 체력바 자동 부착 컴포넌트
    /// </summary>
    public class HealthBarAttachment : MonoBehaviour
    {
        [SerializeField] private GameObject healthBarPrefab;
        [SerializeField] private Vector3 offset = new Vector3(0, 2.2f, 0);

        private WorldHealthBar healthBar;

        private void Start()
        {
            CreateHealthBar();
        }

        private void CreateHealthBar()
        {
            if (healthBarPrefab == null) return;

            GameObject hbObj = Instantiate(healthBarPrefab, transform.position + offset, Quaternion.identity);
            healthBar = hbObj.GetComponent<WorldHealthBar>();

            if (healthBar != null)
            {
                // IDamageable에서 최대 체력 가져오기
                Combat.IDamageable damageable = GetComponent<Combat.IDamageable>();
                float maxHealth = 100f;

                // EnemyAI에서 가져오기
                Combat.EnemyAI enemy = GetComponent<Combat.EnemyAI>();
                if (enemy != null)
                {
                    // EnemyAI에 GetMaxHealth 메서드가 있다고 가정
                    maxHealth = 100f; // 실제 값으로 대체 필요
                }

                healthBar.Initialize(transform, maxHealth);
            }
        }

        /// <summary>
        /// 체력 변경 알림
        /// </summary>
        public void OnHealthChanged(float currentHealth)
        {
            if (healthBar != null)
            {
                healthBar.SetHealth(currentHealth);
            }
        }

        private void OnDestroy()
        {
            if (healthBar != null)
            {
                Destroy(healthBar.gameObject);
            }
        }
    }
}
