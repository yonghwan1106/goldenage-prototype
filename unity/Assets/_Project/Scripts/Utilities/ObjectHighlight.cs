using UnityEngine;

namespace GoldenAge.Utilities
{
    /// <summary>
    /// 오브젝트 하이라이트 효과
    /// </summary>
    public class ObjectHighlight : MonoBehaviour
    {
        [Header("하이라이트 설정")]
        [SerializeField] private bool autoDetectRenderers = true;
        [SerializeField] private Renderer[] targetRenderers;
        [SerializeField] private Color highlightColor = new Color(1f, 0.9f, 0.5f, 1f);
        [SerializeField] private float emissionIntensity = 0.5f;

        [Header("펄스 효과")]
        [SerializeField] private bool usePulse = true;
        [SerializeField] private float pulseSpeed = 2f;
        [SerializeField] private float pulseMin = 0.3f;
        [SerializeField] private float pulseMax = 1f;

        [Header("외곽선 (쉐이더 필요)")]
        [SerializeField] private bool useOutline = false;
        [SerializeField] private Color outlineColor = Color.yellow;
        [SerializeField] private float outlineWidth = 0.02f;

        private Material[] originalMaterials;
        private Material[] highlightMaterials;
        private bool isHighlighted = false;
        private float currentIntensity;

        private static readonly int EmissionColorProperty = Shader.PropertyToID("_EmissionColor");
        private static readonly int OutlineColorProperty = Shader.PropertyToID("_OutlineColor");
        private static readonly int OutlineWidthProperty = Shader.PropertyToID("_OutlineWidth");

        private void Awake()
        {
            if (autoDetectRenderers)
            {
                targetRenderers = GetComponentsInChildren<Renderer>();
            }

            CacheOriginalMaterials();
        }

        private void CacheOriginalMaterials()
        {
            if (targetRenderers == null || targetRenderers.Length == 0) return;

            int count = 0;
            foreach (var r in targetRenderers)
            {
                count += r.materials.Length;
            }

            originalMaterials = new Material[count];
            highlightMaterials = new Material[count];

            int index = 0;
            foreach (var r in targetRenderers)
            {
                foreach (var mat in r.materials)
                {
                    originalMaterials[index] = mat;
                    highlightMaterials[index] = new Material(mat);
                    highlightMaterials[index].EnableKeyword("_EMISSION");
                    index++;
                }
            }
        }

        private void Update()
        {
            if (!isHighlighted || !usePulse) return;

            // 펄스 효과
            float pulse = Mathf.Lerp(pulseMin, pulseMax, (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f);
            currentIntensity = emissionIntensity * pulse;

            UpdateEmission();
        }

        /// <summary>
        /// 하이라이트 켜기
        /// </summary>
        public void EnableHighlight()
        {
            if (isHighlighted) return;
            isHighlighted = true;

            ApplyHighlightMaterials();
            currentIntensity = emissionIntensity;
            UpdateEmission();
        }

        /// <summary>
        /// 하이라이트 끄기
        /// </summary>
        public void DisableHighlight()
        {
            if (!isHighlighted) return;
            isHighlighted = false;

            RestoreOriginalMaterials();
        }

        /// <summary>
        /// 토글
        /// </summary>
        public void ToggleHighlight()
        {
            if (isHighlighted)
                DisableHighlight();
            else
                EnableHighlight();
        }

        private void ApplyHighlightMaterials()
        {
            if (targetRenderers == null) return;

            int index = 0;
            foreach (var r in targetRenderers)
            {
                Material[] mats = new Material[r.materials.Length];
                for (int i = 0; i < r.materials.Length; i++)
                {
                    mats[i] = highlightMaterials[index];
                    index++;
                }
                r.materials = mats;
            }
        }

        private void RestoreOriginalMaterials()
        {
            if (targetRenderers == null) return;

            int index = 0;
            foreach (var r in targetRenderers)
            {
                Material[] mats = new Material[r.materials.Length];
                for (int i = 0; i < r.materials.Length; i++)
                {
                    mats[i] = originalMaterials[index];
                    index++;
                }
                r.materials = mats;
            }
        }

        private void UpdateEmission()
        {
            Color emission = highlightColor * currentIntensity;

            foreach (var mat in highlightMaterials)
            {
                if (mat != null)
                {
                    mat.SetColor(EmissionColorProperty, emission);

                    if (useOutline)
                    {
                        mat.SetColor(OutlineColorProperty, outlineColor);
                        mat.SetFloat(OutlineWidthProperty, outlineWidth);
                    }
                }
            }
        }

        /// <summary>
        /// 하이라이트 색상 변경
        /// </summary>
        public void SetHighlightColor(Color color)
        {
            highlightColor = color;
            if (isHighlighted)
            {
                UpdateEmission();
            }
        }

        private void OnDestroy()
        {
            // 생성된 머티리얼 정리
            if (highlightMaterials != null)
            {
                foreach (var mat in highlightMaterials)
                {
                    if (mat != null)
                    {
                        Destroy(mat);
                    }
                }
            }
        }
    }

    /// <summary>
    /// 상호작용 가능 오브젝트 자동 하이라이트
    /// </summary>
    public class InteractableHighlight : MonoBehaviour
    {
        [SerializeField] private float detectionRange = 3f;
        [SerializeField] private LayerMask interactableLayer;

        private ObjectHighlight currentHighlight;
        private Transform playerTransform;

        private void Start()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
        }

        private void Update()
        {
            if (playerTransform == null) return;

            // 가장 가까운 상호작용 가능 오브젝트 찾기
            Collider[] colliders = Physics.OverlapSphere(playerTransform.position, detectionRange, interactableLayer);

            ObjectHighlight closest = null;
            float closestDist = float.MaxValue;

            foreach (var col in colliders)
            {
                ObjectHighlight highlight = col.GetComponent<ObjectHighlight>();
                if (highlight != null)
                {
                    float dist = Vector3.Distance(playerTransform.position, col.transform.position);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        closest = highlight;
                    }
                }
            }

            // 하이라이트 업데이트
            if (closest != currentHighlight)
            {
                currentHighlight?.DisableHighlight();
                currentHighlight = closest;
                currentHighlight?.EnableHighlight();
            }
        }
    }
}
