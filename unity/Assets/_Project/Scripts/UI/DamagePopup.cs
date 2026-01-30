using UnityEngine;
using TMPro;
using GoldenAge.Core;

namespace GoldenAge.UI
{
    /// <summary>
    /// 데미지 팝업 시스템
    /// </summary>
    public class DamagePopupManager : Singleton<DamagePopupManager>
    {
        [Header("프리팹")]
        [SerializeField] private GameObject popupPrefab;

        [Header("설정")]
        [SerializeField] private float popupDuration = 1f;
        [SerializeField] private float moveSpeed = 2f;
        [SerializeField] private float fadeSpeed = 2f;
        [SerializeField] private float randomOffsetRange = 0.5f;

        [Header("색상")]
        [SerializeField] private Color normalDamageColor = Color.white;
        [SerializeField] private Color criticalDamageColor = Color.yellow;
        [SerializeField] private Color healColor = Color.green;
        [SerializeField] private Color electricColor = new Color(0.3f, 0.7f, 1f);
        [SerializeField] private Color etherColor = new Color(0.6f, 0.3f, 0.9f);
        [SerializeField] private Color fusionColor = new Color(1f, 0.8f, 0.3f);

        [Header("풀링")]
        [SerializeField] private int poolSize = 20;

        private void Start()
        {
            // 풀 등록
            if (popupPrefab != null)
            {
                ObjectPool.Instance?.RegisterPool("DamagePopup", popupPrefab, poolSize, poolSize * 2);
            }
        }

        /// <summary>
        /// 데미지 팝업 표시
        /// </summary>
        public void ShowDamage(Vector3 worldPosition, float damage, DamagePopupType type = DamagePopupType.Normal)
        {
            ShowPopup(worldPosition, Mathf.RoundToInt(damage).ToString(), GetColor(type), GetScale(type));
        }

        /// <summary>
        /// 힐 팝업 표시
        /// </summary>
        public void ShowHeal(Vector3 worldPosition, float amount)
        {
            ShowPopup(worldPosition, $"+{Mathf.RoundToInt(amount)}", healColor, 1f);
        }

        /// <summary>
        /// 텍스트 팝업 표시
        /// </summary>
        public void ShowText(Vector3 worldPosition, string text, Color color)
        {
            ShowPopup(worldPosition, text, color, 1f);
        }

        private void ShowPopup(Vector3 worldPosition, string text, Color color, float scale)
        {
            // 랜덤 오프셋
            Vector3 offset = new Vector3(
                Random.Range(-randomOffsetRange, randomOffsetRange),
                Random.Range(0, randomOffsetRange),
                Random.Range(-randomOffsetRange, randomOffsetRange)
            );

            Vector3 spawnPos = worldPosition + offset;

            // 풀에서 가져오기
            GameObject popup = null;
            if (ObjectPool.Instance != null)
            {
                popup = ObjectPool.Instance.Get("DamagePopup", spawnPos, Quaternion.identity);
            }

            // 풀이 없으면 직접 생성
            if (popup == null && popupPrefab != null)
            {
                popup = Instantiate(popupPrefab, spawnPos, Quaternion.identity);
            }

            if (popup == null) return;

            // 팝업 설정
            DamagePopup popupComponent = popup.GetComponent<DamagePopup>();
            if (popupComponent != null)
            {
                popupComponent.Setup(text, color, scale, popupDuration, moveSpeed, fadeSpeed);
            }
        }

        private Color GetColor(DamagePopupType type)
        {
            return type switch
            {
                DamagePopupType.Critical => criticalDamageColor,
                DamagePopupType.Electric => electricColor,
                DamagePopupType.Ether => etherColor,
                DamagePopupType.Fusion => fusionColor,
                DamagePopupType.Heal => healColor,
                _ => normalDamageColor
            };
        }

        private float GetScale(DamagePopupType type)
        {
            return type switch
            {
                DamagePopupType.Critical => 1.5f,
                DamagePopupType.Fusion => 1.8f,
                _ => 1f
            };
        }
    }

    public enum DamagePopupType
    {
        Normal,
        Critical,
        Electric,
        Ether,
        Fusion,
        Heal
    }

    /// <summary>
    /// 개별 데미지 팝업
    /// </summary>
    public class DamagePopup : MonoBehaviour
    {
        [SerializeField] private TextMeshPro textMesh;
        [SerializeField] private TextMeshProUGUI textMeshUI; // Canvas용

        private float duration;
        private float moveSpeed;
        private float fadeSpeed;
        private float elapsed;
        private Color startColor;
        private Vector3 startScale;

        private void Awake()
        {
            if (textMesh == null)
                textMesh = GetComponent<TextMeshPro>();
            if (textMeshUI == null)
                textMeshUI = GetComponent<TextMeshProUGUI>();
        }

        public void Setup(string text, Color color, float scale, float duration, float moveSpeed, float fadeSpeed)
        {
            this.duration = duration;
            this.moveSpeed = moveSpeed;
            this.fadeSpeed = fadeSpeed;
            this.elapsed = 0f;
            this.startColor = color;
            this.startScale = Vector3.one * scale;

            if (textMesh != null)
            {
                textMesh.text = text;
                textMesh.color = color;
            }

            if (textMeshUI != null)
            {
                textMeshUI.text = text;
                textMeshUI.color = color;
            }

            transform.localScale = startScale;
        }

        private void Update()
        {
            elapsed += Time.deltaTime;

            // 위로 이동
            transform.position += Vector3.up * moveSpeed * Time.deltaTime;

            // 카메라 향하기
            if (Camera.main != null)
            {
                transform.LookAt(Camera.main.transform);
                transform.Rotate(0, 180, 0); // 텍스트 방향 보정
            }

            // 페이드 아웃
            float alpha = 1f - (elapsed / duration);
            Color currentColor = startColor;
            currentColor.a = alpha;

            if (textMesh != null)
                textMesh.color = currentColor;
            if (textMeshUI != null)
                textMeshUI.color = currentColor;

            // 스케일 애니메이션 (처음에 커졌다가 작아짐)
            float scaleMultiplier = elapsed < 0.1f
                ? Mathf.Lerp(1f, 1.2f, elapsed / 0.1f)
                : Mathf.Lerp(1.2f, 0.8f, (elapsed - 0.1f) / (duration - 0.1f));

            transform.localScale = startScale * scaleMultiplier;

            // 시간 종료
            if (elapsed >= duration)
            {
                ReturnToPool();
            }
        }

        private void ReturnToPool()
        {
            PooledObject pooled = GetComponent<PooledObject>();
            if (pooled != null)
            {
                pooled.ReturnToPool();
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}
