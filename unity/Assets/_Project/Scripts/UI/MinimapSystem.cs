using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace GoldenAge.UI
{
    /// <summary>
    /// 미니맵 시스템
    /// </summary>
    public class MinimapSystem : MonoBehaviour
    {
        [Header("카메라 설정")]
        [SerializeField] private Camera minimapCamera;
        [SerializeField] private float cameraHeight = 50f;
        [SerializeField] private float cameraSize = 30f;
        [SerializeField] private bool rotateWithPlayer = true;

        [Header("UI")]
        [SerializeField] private RawImage minimapImage;
        [SerializeField] private RectTransform playerIcon;
        [SerializeField] private Transform markersContainer;

        [Header("마커 프리팹")]
        [SerializeField] private GameObject questMarkerPrefab;
        [SerializeField] private GameObject enemyMarkerPrefab;
        [SerializeField] private GameObject npcMarkerPrefab;
        [SerializeField] private GameObject itemMarkerPrefab;

        [Header("설정")]
        [SerializeField] private float markerVisibleRange = 50f;
        [SerializeField] private float updateInterval = 0.1f;

        private Transform playerTransform;
        private Dictionary<Transform, RectTransform> activeMarkers = new Dictionary<Transform, RectTransform>();
        private float lastUpdateTime;

        private void Start()
        {
            // 플레이어 찾기
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }

            // 미니맵 카메라 설정
            SetupMinimapCamera();
        }

        private void LateUpdate()
        {
            if (playerTransform == null) return;

            // 카메라 위치 업데이트
            UpdateCameraPosition();

            // 마커 업데이트 (일정 간격)
            if (Time.time - lastUpdateTime >= updateInterval)
            {
                UpdateMarkers();
                lastUpdateTime = Time.time;
            }
        }

        private void SetupMinimapCamera()
        {
            if (minimapCamera == null)
            {
                // 미니맵 카메라 생성
                GameObject camObj = new GameObject("MinimapCamera");
                minimapCamera = camObj.AddComponent<Camera>();
                minimapCamera.orthographic = true;
                minimapCamera.orthographicSize = cameraSize;
                minimapCamera.cullingMask = LayerMask.GetMask("Default", "Ground", "Obstacle");
                minimapCamera.clearFlags = CameraClearFlags.SolidColor;
                minimapCamera.backgroundColor = new Color(0.1f, 0.1f, 0.15f);

                // 렌더 텍스처 생성
                RenderTexture rt = new RenderTexture(256, 256, 16);
                minimapCamera.targetTexture = rt;

                if (minimapImage != null)
                {
                    minimapImage.texture = rt;
                }
            }
        }

        private void UpdateCameraPosition()
        {
            if (minimapCamera == null || playerTransform == null) return;

            // 위치
            Vector3 newPos = playerTransform.position;
            newPos.y = cameraHeight;
            minimapCamera.transform.position = newPos;

            // 회전
            if (rotateWithPlayer)
            {
                minimapCamera.transform.rotation = Quaternion.Euler(90f, playerTransform.eulerAngles.y, 0f);
            }
            else
            {
                minimapCamera.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            }
        }

        private void UpdateMarkers()
        {
            // 적 마커
            UpdateMarkersForTag("Enemy", enemyMarkerPrefab);

            // NPC 마커
            UpdateMarkersForTag("NPC", npcMarkerPrefab);

            // 퀘스트 마커
            UpdateQuestMarkers();
        }

        private void UpdateMarkersForTag(string tag, GameObject markerPrefab)
        {
            if (markerPrefab == null || markersContainer == null) return;

            GameObject[] objects = GameObject.FindGameObjectsWithTag(tag);

            foreach (var obj in objects)
            {
                float distance = Vector3.Distance(playerTransform.position, obj.transform.position);

                if (distance <= markerVisibleRange)
                {
                    // 마커가 없으면 생성
                    if (!activeMarkers.ContainsKey(obj.transform))
                    {
                        CreateMarker(obj.transform, markerPrefab);
                    }

                    // 마커 위치 업데이트
                    UpdateMarkerPosition(obj.transform);
                }
                else
                {
                    // 범위 밖이면 마커 제거
                    RemoveMarker(obj.transform);
                }
            }
        }

        private void UpdateQuestMarkers()
        {
            if (questMarkerPrefab == null) return;

            // QuestManager에서 활성 퀘스트 목표 위치 가져오기
            var questManager = Quest.QuestManager.Instance;
            if (questManager == null) return;

            // 퀘스트 목표 마커 처리
            // (실제 구현에서는 QuestManager에서 목표 위치 목록을 가져와야 함)
        }

        private void CreateMarker(Transform target, GameObject prefab)
        {
            if (prefab == null || markersContainer == null) return;

            GameObject marker = Instantiate(prefab, markersContainer);
            RectTransform rt = marker.GetComponent<RectTransform>();

            if (rt != null)
            {
                activeMarkers[target] = rt;
            }
        }

        private void UpdateMarkerPosition(Transform target)
        {
            if (!activeMarkers.ContainsKey(target)) return;

            RectTransform marker = activeMarkers[target];
            if (marker == null) return;

            // 월드 좌표를 미니맵 좌표로 변환
            Vector3 offset = target.position - playerTransform.position;
            float scale = minimapImage.rectTransform.rect.width / (cameraSize * 2f);

            Vector2 markerPos;
            if (rotateWithPlayer)
            {
                // 플레이어 회전 고려
                float angle = -playerTransform.eulerAngles.y * Mathf.Deg2Rad;
                float rotatedX = offset.x * Mathf.Cos(angle) - offset.z * Mathf.Sin(angle);
                float rotatedZ = offset.x * Mathf.Sin(angle) + offset.z * Mathf.Cos(angle);
                markerPos = new Vector2(rotatedX * scale, rotatedZ * scale);
            }
            else
            {
                markerPos = new Vector2(offset.x * scale, offset.z * scale);
            }

            marker.anchoredPosition = markerPos;
        }

        private void RemoveMarker(Transform target)
        {
            if (activeMarkers.ContainsKey(target))
            {
                if (activeMarkers[target] != null)
                {
                    Destroy(activeMarkers[target].gameObject);
                }
                activeMarkers.Remove(target);
            }
        }

        /// <summary>
        /// 퀘스트 마커 추가
        /// </summary>
        public void AddQuestMarker(Vector3 worldPosition, string questId)
        {
            if (questMarkerPrefab == null || markersContainer == null) return;

            GameObject marker = Instantiate(questMarkerPrefab, markersContainer);
            marker.name = $"QuestMarker_{questId}";

            // 위치 업데이트는 UpdateMarkers에서 처리
        }

        /// <summary>
        /// 마커 제거
        /// </summary>
        public void RemoveQuestMarker(string questId)
        {
            Transform marker = markersContainer.Find($"QuestMarker_{questId}");
            if (marker != null)
            {
                Destroy(marker.gameObject);
            }
        }

        /// <summary>
        /// 줌 설정
        /// </summary>
        public void SetZoom(float zoom)
        {
            if (minimapCamera != null)
            {
                minimapCamera.orthographicSize = cameraSize / zoom;
            }
        }

        /// <summary>
        /// 회전 모드 토글
        /// </summary>
        public void ToggleRotation()
        {
            rotateWithPlayer = !rotateWithPlayer;
        }
    }
}
