using UnityEngine;

namespace GoldenAge.Core
{
    /// <summary>
    /// 3인칭 카메라 컨트롤러
    /// (Cinemachine 사용 전 기본 구현)
    /// </summary>
    public class CameraController : MonoBehaviour
    {
        [Header("타겟")]
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 targetOffset = new Vector3(0, 1.5f, 0);

        [Header("거리")]
        [SerializeField] private float distance = 5f;
        [SerializeField] private float minDistance = 2f;
        [SerializeField] private float maxDistance = 10f;
        [SerializeField] private float zoomSpeed = 2f;

        [Header("회전")]
        [SerializeField] private float rotationSpeed = 3f;
        [SerializeField] private float minVerticalAngle = -30f;
        [SerializeField] private float maxVerticalAngle = 60f;

        [Header("부드러움")]
        [SerializeField] private float positionSmoothTime = 0.1f;
        [SerializeField] private float rotationSmoothTime = 0.05f;

        [Header("충돌")]
        [SerializeField] private LayerMask collisionLayers;
        [SerializeField] private float collisionRadius = 0.3f;

        private float currentHorizontalAngle;
        private float currentVerticalAngle = 20f;
        private float currentDistance;
        private Vector3 currentVelocity;

        private PlayerInputActions inputActions;

        private void Start()
        {
            currentDistance = distance;

            if (target == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                    target = player.transform;
            }

            // 입력 설정
            inputActions = new PlayerInputActions();
            inputActions.Player.Enable();

            // 커서 숨기기
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void OnDestroy()
        {
            inputActions?.Player.Disable();
        }

        private void LateUpdate()
        {
            if (target == null) return;

            HandleInput();
            UpdateCameraPosition();
        }

        private void HandleInput()
        {
            // 마우스 회전
            Vector2 lookInput = inputActions.Player.Look.ReadValue<Vector2>();
            currentHorizontalAngle += lookInput.x * rotationSpeed;
            currentVerticalAngle -= lookInput.y * rotationSpeed;
            currentVerticalAngle = Mathf.Clamp(currentVerticalAngle, minVerticalAngle, maxVerticalAngle);

            // 줌 (마우스 휠)
            float scrollInput = Input.GetAxis("Mouse ScrollWheel");
            distance -= scrollInput * zoomSpeed;
            distance = Mathf.Clamp(distance, minDistance, maxDistance);
        }

        private void UpdateCameraPosition()
        {
            // 타겟 위치 (오프셋 적용)
            Vector3 targetPosition = target.position + targetOffset;

            // 카메라 방향 계산
            Quaternion rotation = Quaternion.Euler(currentVerticalAngle, currentHorizontalAngle, 0);
            Vector3 direction = rotation * Vector3.back;

            // 목표 거리
            float targetDistance = distance;

            // 충돌 검사
            if (Physics.SphereCast(targetPosition, collisionRadius, direction, out RaycastHit hit, distance, collisionLayers))
            {
                targetDistance = Mathf.Min(hit.distance - collisionRadius, distance);
            }

            // 부드러운 거리 전환
            currentDistance = Mathf.Lerp(currentDistance, targetDistance, Time.deltaTime * 10f);

            // 최종 위치 계산
            Vector3 desiredPosition = targetPosition + direction * currentDistance;

            // 부드러운 이동
            transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref currentVelocity, positionSmoothTime);

            // 타겟 바라보기
            transform.LookAt(targetPosition);
        }

        /// <summary>
        /// 카메라의 전방 벡터 (Y축 제외)
        /// </summary>
        public Vector3 GetForwardXZ()
        {
            Vector3 forward = transform.forward;
            forward.y = 0;
            return forward.normalized;
        }

        /// <summary>
        /// 카메라의 우측 벡터 (Y축 제외)
        /// </summary>
        public Vector3 GetRightXZ()
        {
            Vector3 right = transform.right;
            right.y = 0;
            return right.normalized;
        }

        /// <summary>
        /// 타겟 설정
        /// </summary>
        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }

        /// <summary>
        /// 커서 토글
        /// </summary>
        public void ToggleCursor(bool show)
        {
            Cursor.lockState = show ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = show;
        }
    }
}
