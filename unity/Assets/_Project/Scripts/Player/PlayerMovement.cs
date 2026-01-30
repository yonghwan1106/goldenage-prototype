using UnityEngine;
using UnityEngine.InputSystem;
using GoldenAge.Core;

namespace GoldenAge.Player
{
    /// <summary>
    /// 플레이어 캐릭터의 3인칭 이동을 담당하는 클래스
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMovement : MonoBehaviour
    {
        [Header("이동 설정")]
        [SerializeField] private float walkSpeed = 4f;
        [SerializeField] private float runSpeed = 7f;
        [SerializeField] private float rotationSmoothTime = 0.1f;
        [SerializeField] private float gravity = -9.81f;

        [Header("참조")]
        [SerializeField] private Transform cameraTransform;

        private CharacterController controller;
        private PlayerInputActions inputActions;

        private Vector2 moveInput;
        private bool isRunning;
        private float currentSpeed;
        private float rotationVelocity;
        private Vector3 verticalVelocity;

        // Properties
        public float CurrentSpeed => currentSpeed;
        public bool IsGrounded => controller != null && controller.isGrounded;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            inputActions = new PlayerInputActions();

            // 카메라 자동 찾기
            if (cameraTransform == null)
            {
                Camera mainCam = Camera.main;
                if (mainCam != null)
                {
                    cameraTransform = mainCam.transform;
                }
            }
        }

        private void OnEnable()
        {
            inputActions.Player.Enable();
            inputActions.Player.Move.performed += OnMovePerformed;
            inputActions.Player.Move.canceled += OnMoveCanceled;
            inputActions.Player.Sprint.performed += _ => isRunning = true;
            inputActions.Player.Sprint.canceled += _ => isRunning = false;
        }

        private void OnDisable()
        {
            inputActions.Player.Move.performed -= OnMovePerformed;
            inputActions.Player.Move.canceled -= OnMoveCanceled;
            inputActions.Player.Disable();
        }

        private void OnMovePerformed(InputAction.CallbackContext ctx)
        {
            moveInput = ctx.ReadValue<Vector2>();
        }

        private void OnMoveCanceled(InputAction.CallbackContext ctx)
        {
            moveInput = Vector2.zero;
        }

        private void Update()
        {
            // 게임 상태 체크
            if (GameManager.Instance != null && !GameManager.Instance.CanPlayerInput())
            {
                currentSpeed = 0f;
                return;
            }

            HandleMovement();
            ApplyGravity();
        }

        private void HandleMovement()
        {
            if (moveInput.sqrMagnitude < 0.01f)
            {
                currentSpeed = 0f;
                return;
            }

            // 카메라 기준 이동 방향 계산
            Vector3 inputDirection = new Vector3(moveInput.x, 0, moveInput.y).normalized;

            float targetAngle = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg;

            if (cameraTransform != null)
            {
                targetAngle += cameraTransform.eulerAngles.y;
            }

            // 부드러운 회전
            float angle = Mathf.SmoothDampAngle(
                transform.eulerAngles.y,
                targetAngle,
                ref rotationVelocity,
                rotationSmoothTime
            );
            transform.rotation = Quaternion.Euler(0, angle, 0);

            // 이동
            currentSpeed = isRunning ? runSpeed : walkSpeed;
            Vector3 moveDirection = Quaternion.Euler(0, targetAngle, 0) * Vector3.forward;
            controller.Move(moveDirection * currentSpeed * Time.deltaTime);
        }

        private void ApplyGravity()
        {
            if (controller.isGrounded && verticalVelocity.y < 0)
            {
                verticalVelocity.y = -2f;
            }

            verticalVelocity.y += gravity * Time.deltaTime;
            controller.Move(verticalVelocity * Time.deltaTime);
        }

        // 외부 접근용 메서드
        public float GetCurrentSpeed() => currentSpeed;
        public bool IsMoving() => moveInput.sqrMagnitude > 0.01f;
        public bool IsRunning() => isRunning && IsMoving();

        /// <summary>
        /// 외부에서 위치 강제 설정 (텔레포트 등)
        /// </summary>
        public void SetPosition(Vector3 position)
        {
            controller.enabled = false;
            transform.position = position;
            controller.enabled = true;
        }

        /// <summary>
        /// 외부에서 회전 설정
        /// </summary>
        public void SetRotation(float yAngle)
        {
            transform.rotation = Quaternion.Euler(0, yAngle, 0);
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            // 이동 방향 표시
            if (Application.isPlaying && IsMoving())
            {
                Gizmos.color = Color.green;
                Gizmos.DrawRay(transform.position + Vector3.up, transform.forward * 2f);
            }
        }
#endif
    }
}
