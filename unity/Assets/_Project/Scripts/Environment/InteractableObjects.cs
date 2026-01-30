using UnityEngine;
using GoldenAge.Combat;

namespace GoldenAge.Environment
{
    /// <summary>
    /// 문 상호작용
    /// </summary>
    public class Door : MonoBehaviour, IInteractable
    {
        [Header("설정")]
        [SerializeField] private bool isLocked = false;
        [SerializeField] private string requiredKeyId = "";
        [SerializeField] private bool autoClose = false;
        [SerializeField] private float autoCloseDelay = 5f;

        [Header("애니메이션")]
        [SerializeField] private Animator animator;
        [SerializeField] private string openTrigger = "Open";
        [SerializeField] private string closeTrigger = "Close";

        [Header("회전 방식 (Animator 없을 때)")]
        [SerializeField] private bool useRotation = true;
        [SerializeField] private float openAngle = 90f;
        [SerializeField] private float openSpeed = 3f;
        [SerializeField] private Transform pivotPoint;

        [Header("사운드")]
        [SerializeField] private AudioClip openSound;
        [SerializeField] private AudioClip closeSound;
        [SerializeField] private AudioClip lockedSound;

        private bool isOpen = false;
        private float targetAngle = 0f;
        private float currentAngle = 0f;
        private Coroutine autoCloseCoroutine;

        public bool IsOpen => isOpen;
        public bool IsLocked => isLocked;

        public string GetInteractionPrompt()
        {
            if (isLocked) return "잠김";
            return isOpen ? "닫기" : "열기";
        }

        public void OnInteract(GameObject interactor)
        {
            if (isLocked)
            {
                // 열쇠 확인
                if (!string.IsNullOrEmpty(requiredKeyId))
                {
                    // 인벤토리에서 열쇠 확인 (구현 필요)
                    bool hasKey = false; // CheckInventoryForKey(interactor, requiredKeyId);

                    if (hasKey)
                    {
                        Unlock();
                        Toggle();
                        return;
                    }
                }

                PlaySound(lockedSound);
                UI.NotificationSystem.Instance?.ShowWarning("잠겨 있습니다");
                return;
            }

            Toggle();
        }

        public void Toggle()
        {
            if (isOpen)
                Close();
            else
                Open();
        }

        public void Open()
        {
            if (isOpen) return;
            isOpen = true;

            if (animator != null)
            {
                animator.SetTrigger(openTrigger);
            }
            else if (useRotation)
            {
                targetAngle = openAngle;
            }

            PlaySound(openSound);

            if (autoClose)
            {
                if (autoCloseCoroutine != null)
                    StopCoroutine(autoCloseCoroutine);
                autoCloseCoroutine = StartCoroutine(AutoCloseCoroutine());
            }
        }

        public void Close()
        {
            if (!isOpen) return;
            isOpen = false;

            if (animator != null)
            {
                animator.SetTrigger(closeTrigger);
            }
            else if (useRotation)
            {
                targetAngle = 0f;
            }

            PlaySound(closeSound);

            if (autoCloseCoroutine != null)
            {
                StopCoroutine(autoCloseCoroutine);
                autoCloseCoroutine = null;
            }
        }

        public void Lock()
        {
            isLocked = true;
        }

        public void Unlock()
        {
            isLocked = false;
            UI.NotificationSystem.Instance?.ShowSuccess("잠금 해제됨");
        }

        private void Update()
        {
            if (useRotation && animator == null)
            {
                if (!Mathf.Approximately(currentAngle, targetAngle))
                {
                    currentAngle = Mathf.MoveTowards(currentAngle, targetAngle, openSpeed * 60f * Time.deltaTime);

                    Transform pivot = pivotPoint != null ? pivotPoint : transform;
                    pivot.localRotation = Quaternion.Euler(0, currentAngle, 0);
                }
            }
        }

        private System.Collections.IEnumerator AutoCloseCoroutine()
        {
            yield return new WaitForSeconds(autoCloseDelay);
            Close();
        }

        private void PlaySound(AudioClip clip)
        {
            if (clip != null)
            {
                AudioSource.PlayClipAtPoint(clip, transform.position);
            }
        }
    }

    /// <summary>
    /// 레버/스위치 상호작용
    /// </summary>
    public class Lever : MonoBehaviour, IInteractable
    {
        [Header("설정")]
        [SerializeField] private bool isOn = false;
        [SerializeField] private bool canToggle = true;
        [SerializeField] private bool oneTimeUse = false;

        [Header("연결된 오브젝트")]
        [SerializeField] private GameObject[] targetObjects;
        [SerializeField] private UnityEngine.Events.UnityEvent onActivate;
        [SerializeField] private UnityEngine.Events.UnityEvent onDeactivate;

        [Header("시각화")]
        [SerializeField] private Transform leverHandle;
        [SerializeField] private float onAngle = 45f;
        [SerializeField] private float offAngle = -45f;
        [SerializeField] private float rotateSpeed = 5f;

        [Header("사운드")]
        [SerializeField] private AudioClip toggleSound;

        private bool hasBeenUsed = false;
        private float currentAngle;
        private float targetAngle;

        public bool IsOn => isOn;

        private void Start()
        {
            currentAngle = isOn ? onAngle : offAngle;
            targetAngle = currentAngle;

            if (leverHandle != null)
            {
                leverHandle.localRotation = Quaternion.Euler(currentAngle, 0, 0);
            }
        }

        public string GetInteractionPrompt()
        {
            if (oneTimeUse && hasBeenUsed) return "사용됨";
            return isOn ? "끄기" : "켜기";
        }

        public void OnInteract(GameObject interactor)
        {
            if (oneTimeUse && hasBeenUsed) return;

            if (canToggle)
            {
                Toggle();
            }
            else if (!isOn)
            {
                Activate();
            }
        }

        public void Toggle()
        {
            if (isOn)
                Deactivate();
            else
                Activate();
        }

        public void Activate()
        {
            if (isOn) return;

            isOn = true;
            hasBeenUsed = true;
            targetAngle = onAngle;

            PlaySound();

            // 이벤트 발동
            onActivate?.Invoke();

            // 연결된 오브젝트 활성화
            foreach (var obj in targetObjects)
            {
                if (obj != null)
                {
                    IActivatable activatable = obj.GetComponent<IActivatable>();
                    if (activatable != null)
                    {
                        activatable.Activate();
                    }
                    else
                    {
                        obj.SetActive(true);
                    }
                }
            }
        }

        public void Deactivate()
        {
            if (!isOn || !canToggle) return;

            isOn = false;
            targetAngle = offAngle;

            PlaySound();

            onDeactivate?.Invoke();

            foreach (var obj in targetObjects)
            {
                if (obj != null)
                {
                    IActivatable activatable = obj.GetComponent<IActivatable>();
                    if (activatable != null)
                    {
                        activatable.Deactivate();
                    }
                    else
                    {
                        obj.SetActive(false);
                    }
                }
            }
        }

        private void Update()
        {
            if (leverHandle != null && !Mathf.Approximately(currentAngle, targetAngle))
            {
                currentAngle = Mathf.MoveTowards(currentAngle, targetAngle, rotateSpeed * 60f * Time.deltaTime);
                leverHandle.localRotation = Quaternion.Euler(currentAngle, 0, 0);
            }
        }

        private void PlaySound()
        {
            if (toggleSound != null)
            {
                AudioSource.PlayClipAtPoint(toggleSound, transform.position);
            }
        }
    }

    /// <summary>
    /// 압력판
    /// </summary>
    public class PressurePlate : MonoBehaviour
    {
        [Header("설정")]
        [SerializeField] private bool requiresPlayer = true;
        [SerializeField] private float activationWeight = 1f;

        [Header("연결된 오브젝트")]
        [SerializeField] private GameObject[] targetObjects;
        [SerializeField] private UnityEngine.Events.UnityEvent onActivate;
        [SerializeField] private UnityEngine.Events.UnityEvent onDeactivate;

        [Header("시각화")]
        [SerializeField] private Transform plateVisual;
        [SerializeField] private float pressDepth = 0.1f;

        [Header("사운드")]
        [SerializeField] private AudioClip pressSound;
        [SerializeField] private AudioClip releaseSound;

        private bool isPressed = false;
        private int objectsOnPlate = 0;

        public bool IsPressed => isPressed;

        private void OnTriggerEnter(Collider other)
        {
            if (requiresPlayer && !other.CompareTag("Player")) return;

            objectsOnPlate++;

            if (!isPressed)
            {
                Activate();
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (requiresPlayer && !other.CompareTag("Player")) return;

            objectsOnPlate = Mathf.Max(0, objectsOnPlate - 1);

            if (objectsOnPlate == 0 && isPressed)
            {
                Deactivate();
            }
        }

        private void Activate()
        {
            isPressed = true;

            // 시각 효과
            if (plateVisual != null)
            {
                plateVisual.localPosition = new Vector3(0, -pressDepth, 0);
            }

            if (pressSound != null)
            {
                AudioSource.PlayClipAtPoint(pressSound, transform.position);
            }

            onActivate?.Invoke();

            foreach (var obj in targetObjects)
            {
                if (obj != null)
                {
                    IActivatable activatable = obj.GetComponent<IActivatable>();
                    activatable?.Activate();
                }
            }
        }

        private void Deactivate()
        {
            isPressed = false;

            if (plateVisual != null)
            {
                plateVisual.localPosition = Vector3.zero;
            }

            if (releaseSound != null)
            {
                AudioSource.PlayClipAtPoint(releaseSound, transform.position);
            }

            onDeactivate?.Invoke();

            foreach (var obj in targetObjects)
            {
                if (obj != null)
                {
                    IActivatable activatable = obj.GetComponent<IActivatable>();
                    activatable?.Deactivate();
                }
            }
        }
    }

    /// <summary>
    /// 활성화 가능 인터페이스
    /// </summary>
    public interface IActivatable
    {
        void Activate();
        void Deactivate();
    }

    /// <summary>
    /// 이동 플랫폼
    /// </summary>
    public class MovingPlatform : MonoBehaviour, IActivatable
    {
        [Header("경로")]
        [SerializeField] private Transform[] waypoints;
        [SerializeField] private float moveSpeed = 3f;
        [SerializeField] private float waitTime = 1f;

        [Header("설정")]
        [SerializeField] private bool startActive = true;
        [SerializeField] private bool loop = true;
        [SerializeField] private bool pingPong = true;

        private int currentWaypointIndex = 0;
        private bool isMoving = false;
        private bool movingForward = true;
        private float waitTimer = 0f;

        private void Start()
        {
            if (startActive)
            {
                isMoving = true;
            }

            if (waypoints.Length > 0)
            {
                transform.position = waypoints[0].position;
            }
        }

        private void Update()
        {
            if (!isMoving || waypoints.Length < 2) return;

            // 대기 중
            if (waitTimer > 0)
            {
                waitTimer -= Time.deltaTime;
                return;
            }

            // 이동
            Transform target = waypoints[currentWaypointIndex];
            transform.position = Vector3.MoveTowards(transform.position, target.position, moveSpeed * Time.deltaTime);

            // 도착 체크
            if (Vector3.Distance(transform.position, target.position) < 0.01f)
            {
                waitTimer = waitTime;

                // 다음 웨이포인트
                if (pingPong)
                {
                    if (movingForward)
                    {
                        currentWaypointIndex++;
                        if (currentWaypointIndex >= waypoints.Length)
                        {
                            currentWaypointIndex = waypoints.Length - 2;
                            movingForward = false;
                        }
                    }
                    else
                    {
                        currentWaypointIndex--;
                        if (currentWaypointIndex < 0)
                        {
                            currentWaypointIndex = 1;
                            movingForward = true;
                        }
                    }
                }
                else if (loop)
                {
                    currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
                }
                else
                {
                    currentWaypointIndex++;
                    if (currentWaypointIndex >= waypoints.Length)
                    {
                        isMoving = false;
                    }
                }
            }
        }

        public void Activate()
        {
            isMoving = true;
        }

        public void Deactivate()
        {
            isMoving = false;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                other.transform.SetParent(transform);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                other.transform.SetParent(null);
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (waypoints == null || waypoints.Length == 0) return;

            Gizmos.color = Color.cyan;
            for (int i = 0; i < waypoints.Length; i++)
            {
                if (waypoints[i] == null) continue;

                Gizmos.DrawWireSphere(waypoints[i].position, 0.3f);

                if (i < waypoints.Length - 1 && waypoints[i + 1] != null)
                {
                    Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
                }
            }

            if (loop && waypoints.Length > 1)
            {
                Gizmos.DrawLine(waypoints[waypoints.Length - 1].position, waypoints[0].position);
            }
        }
    }
}
