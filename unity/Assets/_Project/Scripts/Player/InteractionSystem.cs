using UnityEngine;
using UnityEngine.InputSystem;
using GoldenAge.Core;

namespace GoldenAge.Player
{
    /// <summary>
    /// 상호작용 가능 인터페이스
    /// </summary>
    public interface IInteractable
    {
        void OnSelect();              // 범위 내 진입 시
        void OnDeselect();            // 범위 이탈 시
        void Interact();              // E키 입력 시
        string GetInteractionPrompt(); // UI 표시용 ("E: 대화하기")
    }

    /// <summary>
    /// 플레이어 상호작용 시스템
    /// </summary>
    public class InteractionSystem : MonoBehaviour
    {
        [Header("설정")]
        [SerializeField] private float interactionRange = 2f;
        [SerializeField] private LayerMask interactableLayer;
        [SerializeField] private Transform interactionPoint;

        [Header("UI")]
        [SerializeField] private GameObject interactionPromptUI;
        [SerializeField] private TMPro.TMP_Text promptText;

        private PlayerInputActions inputActions;
        private IInteractable currentTarget;

        public IInteractable CurrentTarget => currentTarget;

        private void Awake()
        {
            inputActions = new PlayerInputActions();

            if (interactionPoint == null)
            {
                interactionPoint = transform;
            }
        }

        private void OnEnable()
        {
            inputActions.Player.Enable();
            inputActions.Player.Interact.performed += OnInteract;
        }

        private void OnDisable()
        {
            inputActions.Player.Interact.performed -= OnInteract;
            inputActions.Player.Disable();
        }

        private void Update()
        {
            if (GameManager.Instance != null && !GameManager.Instance.CanPlayerInput())
            {
                if (currentTarget != null)
                {
                    currentTarget.OnDeselect();
                    currentTarget = null;
                    UpdatePromptUI();
                }
                return;
            }

            DetectInteractable();
        }

        private void DetectInteractable()
        {
            Collider[] hits = Physics.OverlapSphere(
                interactionPoint.position,
                interactionRange,
                interactableLayer
            );

            IInteractable nearest = null;
            float nearestDistance = float.MaxValue;

            foreach (var hit in hits)
            {
                IInteractable interactable = hit.GetComponent<IInteractable>();
                if (interactable != null)
                {
                    float distance = Vector3.Distance(interactionPoint.position, hit.transform.position);
                    if (distance < nearestDistance)
                    {
                        nearestDistance = distance;
                        nearest = interactable;
                    }
                }
            }

            // 타겟 변경 체크
            if (nearest != currentTarget)
            {
                currentTarget?.OnDeselect();
                currentTarget = nearest;
                currentTarget?.OnSelect();
                UpdatePromptUI();
            }
        }

        private void OnInteract(InputAction.CallbackContext ctx)
        {
            if (ctx.performed && currentTarget != null)
            {
                if (GameManager.Instance == null || GameManager.Instance.CanPlayerInput())
                {
                    currentTarget.Interact();
                }
            }
        }

        private void UpdatePromptUI()
        {
            if (interactionPromptUI != null)
            {
                if (currentTarget != null)
                {
                    interactionPromptUI.SetActive(true);
                    if (promptText != null)
                    {
                        promptText.text = currentTarget.GetInteractionPrompt();
                    }
                }
                else
                {
                    interactionPromptUI.SetActive(false);
                }
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Transform point = interactionPoint != null ? interactionPoint : transform;
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(point.position, interactionRange);
        }
#endif
    }
}
