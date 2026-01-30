using UnityEngine;
using GoldenAge.Player;
using GoldenAge.Combat;

namespace GoldenAge.Core
{
    /// <summary>
    /// 게임 씬 초기화 담당
    /// </summary>
    public class GameInitializer : MonoBehaviour
    {
        [Header("플레이어")]
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private Transform playerSpawnPoint;

        [Header("카메라")]
        [SerializeField] private GameObject cameraPrefab;

        [Header("UI")]
        [SerializeField] private GameObject hudPrefab;

        [Header("초기화 순서")]
        [SerializeField] private bool spawnPlayer = true;
        [SerializeField] private bool setupCamera = true;
        [SerializeField] private bool setupUI = true;

        private GameObject playerInstance;
        private GameObject cameraInstance;
        private GameObject hudInstance;

        private void Start()
        {
            Initialize();
        }

        public void Initialize()
        {
            Debug.Log("[GameInitializer] 게임 초기화 시작...");

            // 1. 플레이어 스폰
            if (spawnPlayer)
            {
                SpawnPlayer();
            }

            // 2. 카메라 설정
            if (setupCamera)
            {
                SetupCamera();
            }

            // 3. UI 설정
            if (setupUI)
            {
                SetupUI();
            }

            // 4. 게임 상태 설정
            GameManager.Instance?.SetState(GameState.Playing);

            // 5. 커서 설정
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            Debug.Log("[GameInitializer] 게임 초기화 완료!");
        }

        private void SpawnPlayer()
        {
            // 기존 플레이어 확인
            playerInstance = GameObject.FindGameObjectWithTag("Player");

            if (playerInstance != null)
            {
                Debug.Log("[GameInitializer] 기존 플레이어 발견, 위치 재설정");
                if (playerSpawnPoint != null)
                {
                    playerInstance.transform.position = playerSpawnPoint.position;
                    playerInstance.transform.rotation = playerSpawnPoint.rotation;
                }
                return;
            }

            // 새 플레이어 생성
            if (playerPrefab != null)
            {
                Vector3 spawnPos = playerSpawnPoint != null
                    ? playerSpawnPoint.position
                    : Vector3.zero;

                Quaternion spawnRot = playerSpawnPoint != null
                    ? playerSpawnPoint.rotation
                    : Quaternion.identity;

                playerInstance = Instantiate(playerPrefab, spawnPos, spawnRot);
                playerInstance.name = "Player";
                Debug.Log("[GameInitializer] 플레이어 생성됨");
            }
            else
            {
                Debug.LogWarning("[GameInitializer] 플레이어 프리팹이 없습니다!");
            }
        }

        private void SetupCamera()
        {
            // 기존 메인 카메라 확인
            Camera mainCam = Camera.main;

            if (mainCam != null)
            {
                // CameraController 설정
                CameraController camController = mainCam.GetComponent<CameraController>();
                if (camController != null && playerInstance != null)
                {
                    camController.SetTarget(playerInstance.transform);
                }
                return;
            }

            // 새 카메라 생성
            if (cameraPrefab != null)
            {
                cameraInstance = Instantiate(cameraPrefab);
                cameraInstance.name = "Main Camera";

                CameraController controller = cameraInstance.GetComponent<CameraController>();
                if (controller != null && playerInstance != null)
                {
                    controller.SetTarget(playerInstance.transform);
                }

                Debug.Log("[GameInitializer] 카메라 생성됨");
            }
        }

        private void SetupUI()
        {
            // 기존 HUD 확인
            if (FindObjectOfType<UI.GameHUD>() != null)
            {
                Debug.Log("[GameInitializer] 기존 HUD 발견");
                return;
            }

            // 새 HUD 생성
            if (hudPrefab != null)
            {
                hudInstance = Instantiate(hudPrefab);
                hudInstance.name = "GameHUD";
                Debug.Log("[GameInitializer] HUD 생성됨");
            }
        }

        /// <summary>
        /// 플레이어 리스폰
        /// </summary>
        public void RespawnPlayer()
        {
            if (playerInstance != null && playerSpawnPoint != null)
            {
                playerInstance.transform.position = playerSpawnPoint.position;
                playerInstance.transform.rotation = playerSpawnPoint.rotation;

                // 체력 회복
                PlayerStats stats = playerInstance.GetComponent<PlayerStats>();
                if (stats != null)
                {
                    stats.Heal(stats.MaxHealth);
                    stats.RestoreEnergy(stats.MaxEnergy);
                }

                Debug.Log("[GameInitializer] 플레이어 리스폰");
            }
        }

        /// <summary>
        /// 스폰 포인트 설정
        /// </summary>
        public void SetSpawnPoint(Transform newSpawnPoint)
        {
            playerSpawnPoint = newSpawnPoint;
        }
    }
}
