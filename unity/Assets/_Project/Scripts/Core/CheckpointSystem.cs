using UnityEngine;
using System.Collections.Generic;

namespace GoldenAge.Core
{
    /// <summary>
    /// 체크포인트/리스폰 시스템
    /// </summary>
    public class CheckpointSystem : Singleton<CheckpointSystem>
    {
        [Header("설정")]
        [SerializeField] private bool autoSaveOnCheckpoint = true;
        [SerializeField] private float respawnDelay = 2f;

        [Header("이펙트")]
        [SerializeField] private GameObject checkpointActivateVFX;
        [SerializeField] private GameObject respawnVFX;
        [SerializeField] private AudioClip checkpointSound;
        [SerializeField] private AudioClip respawnSound;

        private Checkpoint currentCheckpoint;
        private List<Checkpoint> allCheckpoints = new List<Checkpoint>();
        private Vector3 lastSafePosition;
        private Quaternion lastSafeRotation;

        public Checkpoint CurrentCheckpoint => currentCheckpoint;
        public Vector3 RespawnPosition => currentCheckpoint != null
            ? currentCheckpoint.RespawnPosition
            : lastSafePosition;

        public event System.Action<Checkpoint> OnCheckpointActivated;
        public event System.Action OnPlayerRespawn;

        protected override void Awake()
        {
            base.Awake();

            // 기본 리스폰 위치
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                lastSafePosition = player.transform.position;
                lastSafeRotation = player.transform.rotation;
            }
        }

        /// <summary>
        /// 체크포인트 등록
        /// </summary>
        public void RegisterCheckpoint(Checkpoint checkpoint)
        {
            if (!allCheckpoints.Contains(checkpoint))
            {
                allCheckpoints.Add(checkpoint);
            }
        }

        /// <summary>
        /// 체크포인트 해제
        /// </summary>
        public void UnregisterCheckpoint(Checkpoint checkpoint)
        {
            allCheckpoints.Remove(checkpoint);
        }

        /// <summary>
        /// 체크포인트 활성화
        /// </summary>
        public void ActivateCheckpoint(Checkpoint checkpoint)
        {
            if (checkpoint == currentCheckpoint) return;

            // 이전 체크포인트 비활성화
            if (currentCheckpoint != null)
            {
                currentCheckpoint.SetActive(false);
            }

            currentCheckpoint = checkpoint;
            currentCheckpoint.SetActive(true);

            // 안전 위치 업데이트
            lastSafePosition = checkpoint.RespawnPosition;
            lastSafeRotation = checkpoint.RespawnRotation;

            // 이펙트
            if (checkpointActivateVFX != null)
            {
                Instantiate(checkpointActivateVFX, checkpoint.transform.position, Quaternion.identity);
            }

            if (checkpointSound != null)
            {
                AudioSource.PlayClipAtPoint(checkpointSound, checkpoint.transform.position);
            }

            // 자동 저장
            if (autoSaveOnCheckpoint)
            {
                SaveSystem.Instance?.SaveGame();
            }

            OnCheckpointActivated?.Invoke(checkpoint);

            // 알림
            UI.NotificationSystem.Instance?.ShowSuccess("체크포인트 도달");

            Debug.Log($"[Checkpoint] 활성화: {checkpoint.CheckpointName}");
        }

        /// <summary>
        /// 플레이어 리스폰
        /// </summary>
        public void RespawnPlayer()
        {
            StartCoroutine(RespawnCoroutine());
        }

        private System.Collections.IEnumerator RespawnCoroutine()
        {
            // 페이드 아웃 (SceneLoader 사용)
            // SceneLoader.Instance?.FadeOut();

            yield return new WaitForSeconds(respawnDelay);

            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                // 위치 이동
                CharacterController cc = player.GetComponent<CharacterController>();
                if (cc != null)
                {
                    cc.enabled = false;
                    player.transform.position = RespawnPosition;
                    player.transform.rotation = lastSafeRotation;
                    cc.enabled = true;
                }
                else
                {
                    player.transform.position = RespawnPosition;
                    player.transform.rotation = lastSafeRotation;
                }

                // 체력 회복
                Player.PlayerStats stats = player.GetComponent<Player.PlayerStats>();
                if (stats != null)
                {
                    stats.Heal(stats.MaxHealth * 0.5f); // 50% 체력으로 리스폰
                    stats.RestoreEnergy(stats.MaxEnergy * 0.5f);
                }

                // 이펙트
                if (respawnVFX != null)
                {
                    Instantiate(respawnVFX, RespawnPosition, Quaternion.identity);
                }

                if (respawnSound != null)
                {
                    AudioSource.PlayClipAtPoint(respawnSound, RespawnPosition);
                }
            }

            OnPlayerRespawn?.Invoke();

            // 페이드 인
            // SceneLoader.Instance?.FadeIn();

            Debug.Log("[Checkpoint] 플레이어 리스폰됨");
        }

        /// <summary>
        /// 안전 위치 수동 설정
        /// </summary>
        public void SetSafePosition(Vector3 position, Quaternion rotation)
        {
            lastSafePosition = position;
            lastSafeRotation = rotation;
        }

        /// <summary>
        /// 특정 체크포인트로 이동
        /// </summary>
        public void TeleportToCheckpoint(string checkpointName)
        {
            Checkpoint target = allCheckpoints.Find(c => c.CheckpointName == checkpointName);
            if (target != null)
            {
                ActivateCheckpoint(target);

                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    player.transform.position = target.RespawnPosition;
                    player.transform.rotation = target.RespawnRotation;
                }
            }
        }

        /// <summary>
        /// 모든 체크포인트 리셋
        /// </summary>
        public void ResetAllCheckpoints()
        {
            foreach (var cp in allCheckpoints)
            {
                cp.SetActive(false);
            }
            currentCheckpoint = null;
        }
    }

    /// <summary>
    /// 체크포인트 컴포넌트
    /// </summary>
    public class Checkpoint : MonoBehaviour
    {
        [Header("체크포인트 정보")]
        [SerializeField] private string checkpointName = "Checkpoint";
        [SerializeField] private Transform respawnPoint;

        [Header("트리거")]
        [SerializeField] private bool useColliderTrigger = true;
        [SerializeField] private float activationRadius = 2f;

        [Header("시각화")]
        [SerializeField] private GameObject activeVisual;
        [SerializeField] private GameObject inactiveVisual;
        [SerializeField] private Light checkpointLight;
        [SerializeField] private Color activeColor = Color.green;
        [SerializeField] private Color inactiveColor = Color.gray;

        private bool isActive = false;

        public string CheckpointName => checkpointName;
        public bool IsActive => isActive;
        public Vector3 RespawnPosition => respawnPoint != null
            ? respawnPoint.position
            : transform.position;
        public Quaternion RespawnRotation => respawnPoint != null
            ? respawnPoint.rotation
            : transform.rotation;

        private void Start()
        {
            CheckpointSystem.Instance?.RegisterCheckpoint(this);
            UpdateVisuals();
        }

        private void OnDestroy()
        {
            CheckpointSystem.Instance?.UnregisterCheckpoint(this);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!useColliderTrigger) return;

            if (other.CompareTag("Player"))
            {
                CheckpointSystem.Instance?.ActivateCheckpoint(this);
            }
        }

        /// <summary>
        /// 수동 활성화
        /// </summary>
        public void Activate()
        {
            CheckpointSystem.Instance?.ActivateCheckpoint(this);
        }

        /// <summary>
        /// 활성 상태 설정
        /// </summary>
        public void SetActive(bool active)
        {
            isActive = active;
            UpdateVisuals();
        }

        private void UpdateVisuals()
        {
            if (activeVisual != null)
                activeVisual.SetActive(isActive);

            if (inactiveVisual != null)
                inactiveVisual.SetActive(!isActive);

            if (checkpointLight != null)
            {
                checkpointLight.color = isActive ? activeColor : inactiveColor;
                checkpointLight.intensity = isActive ? 2f : 0.5f;
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = isActive ? activeColor : inactiveColor;
            Gizmos.DrawWireSphere(transform.position, activationRadius);

            if (respawnPoint != null)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(transform.position, respawnPoint.position);
                Gizmos.DrawWireSphere(respawnPoint.position, 0.3f);

                // 방향 표시
                Gizmos.DrawRay(respawnPoint.position, respawnPoint.forward * 2f);
            }
        }
    }
}
