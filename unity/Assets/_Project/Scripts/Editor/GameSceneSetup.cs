using UnityEngine;
using UnityEditor;
using UnityEditor.AI;
using UnityEngine.AI;

namespace GoldenAge.Editor
{
    /// <summary>
    /// 게임 씬 설정 도구 - 필수 시스템 자동 배치
    /// </summary>
    public static class GameSceneSetup
    {
        [MenuItem("GoldenAge/Scene/Add All Game Systems")]
        public static void AddAllGameSystems()
        {
            AddCoreManagers();
            AddPlayerSystems();
            AddUISystems();
            AddDebugSystems();

            Debug.Log("[GameSceneSetup] 모든 게임 시스템 추가 완료!");
        }

        [MenuItem("GoldenAge/Scene/Add Core Managers")]
        public static void AddCoreManagers()
        {
            // GameSystems 오브젝트 찾기 또는 생성
            var gameSystems = GameObject.Find("GameSystems");
            if (gameSystems == null)
            {
                gameSystems = new GameObject("GameSystems");
            }

            // GameManager
            AddComponentIfMissing<Core.GameManager>(gameSystems);

            // AudioManager
            AddComponentIfMissing<Core.AudioManager>(gameSystems);

            // SaveSystem
            AddComponentIfMissing<Core.SaveSystem>(gameSystems);

            // SceneLoader
            AddComponentIfMissing<Core.SceneLoader>(gameSystems);

            // GameSettings
            AddComponentIfMissing<Core.GameSettings>(gameSystems);

            // ObjectPool
            AddComponentIfMissing<Core.ObjectPool>(gameSystems);

            // VFXManager
            AddComponentIfMissing<Core.VFXManager>(gameSystems);

            // QuickSaveSystem
            AddComponentIfMissing<Core.QuickSaveSystem>(gameSystems);

            // InventorySystem (Items 네임스페이스)
            AddComponentIfMissing<Items.InventorySystem>(gameSystems);

            Debug.Log("[GameSceneSetup] 코어 매니저 추가 완료");
        }

        [MenuItem("GoldenAge/Scene/Add Player Systems")]
        public static void AddPlayerSystems()
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                Debug.LogWarning("[GameSceneSetup] 플레이어가 없습니다. 먼저 플레이어를 배치하세요.");
                return;
            }

            // BuffSystem
            AddComponentIfMissing<Core.BuffSystem>(player);

            // FootstepSystem
            AddComponentIfMissing<Player.FootstepSystem>(player);

            Debug.Log("[GameSceneSetup] 플레이어 시스템 추가 완료");
        }

        [MenuItem("GoldenAge/Scene/Add UI Systems")]
        public static void AddUISystems()
        {
            // Canvas 찾기
            var canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                Debug.LogWarning("[GameSceneSetup] Canvas가 없습니다.");
                return;
            }

            // UI 매니저 오브젝트
            var uiManager = GameObject.Find("UIManager");
            if (uiManager == null)
            {
                uiManager = new GameObject("UIManager");
                uiManager.transform.SetParent(canvas.transform, false);
            }

            // BuffUI
            var buffUI = GameObject.Find("BuffUI");
            if (buffUI == null)
            {
                buffUI = new GameObject("BuffUI");
                buffUI.transform.SetParent(canvas.transform, false);
                buffUI.AddComponent<UI.BuffUI>();

                var rect = buffUI.AddComponent<RectTransform>();
                rect.anchorMin = new Vector2(0, 1);
                rect.anchorMax = new Vector2(0, 1);
                rect.pivot = new Vector2(0, 1);
                rect.anchoredPosition = new Vector2(10, -100);
                rect.sizeDelta = new Vector2(300, 50);
            }

            // OptionsMenu
            var optionsMenu = GameObject.Find("OptionsMenu");
            if (optionsMenu == null)
            {
                optionsMenu = new GameObject("OptionsMenu");
                optionsMenu.transform.SetParent(canvas.transform, false);
                optionsMenu.AddComponent<UI.OptionsMenu>();
            }

            Debug.Log("[GameSceneSetup] UI 시스템 추가 완료");
        }

        [MenuItem("GoldenAge/Scene/Add Debug Systems")]
        public static void AddDebugSystems()
        {
            var debugSystems = GameObject.Find("DebugSystems");
            if (debugSystems == null)
            {
                debugSystems = new GameObject("DebugSystems");
            }

            // CheatCommands
            AddComponentIfMissing<Utilities.CheatCommands>(debugSystems);

            Debug.Log("[GameSceneSetup] 디버그 시스템 추가 완료");
        }

        [MenuItem("GoldenAge/Scene/Bake NavMesh")]
        public static void BakeNavMesh()
        {
            // 씬의 모든 MeshRenderer를 찾아서 Navigation Static으로 설정
            var meshRenderers = Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None);
            int markedCount = 0;

            foreach (var renderer in meshRenderers)
            {
                // 바닥, 벽, 장애물 등을 Navigation Static으로 설정
                var go = renderer.gameObject;
                string nameLower = go.name.ToLower();

                if (nameLower.Contains("floor") || nameLower.Contains("ground") ||
                    nameLower.Contains("wall") || nameLower.Contains("platform") ||
                    nameLower.Contains("terrain") || nameLower.Contains("cube") ||
                    nameLower.Contains("plane") || go.isStatic)
                {
                    GameObjectUtility.SetStaticEditorFlags(go,
                        GameObjectUtility.GetStaticEditorFlags(go) | StaticEditorFlags.NavigationStatic);
                    markedCount++;
                }
            }

            Debug.Log($"[GameSceneSetup] {markedCount}개 오브젝트를 Navigation Static으로 설정");

            // 레거시 NavMesh 베이크
#pragma warning disable CS0618
            UnityEditor.AI.NavMeshBuilder.BuildNavMesh();
#pragma warning restore CS0618

            Debug.Log("[GameSceneSetup] NavMesh 베이크 완료!");
        }

        [MenuItem("GoldenAge/Scene/Setup Enemy Navigation")]
        public static void SetupEnemyNavigation()
        {
            // 먼저 NavMesh 베이크
            BakeNavMesh();

            // 모든 적 찾기
            var enemies = Object.FindObjectsByType<NavMeshAgent>(FindObjectsSortMode.None);
            int count = 0;

            foreach (var agent in enemies)
            {
                // NavMeshAgent 설정 최적화
                agent.speed = 3.5f;
                agent.angularSpeed = 120f;
                agent.acceleration = 8f;
                agent.stoppingDistance = 1.5f;
                agent.autoBraking = true;

                EditorUtility.SetDirty(agent);
                count++;
            }

            Debug.Log($"[GameSceneSetup] {count}개의 NavMeshAgent 설정 완료");
        }

        [MenuItem("GoldenAge/Test/Start Auto Play Test")]
        public static void StartAutoPlayTest()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("[GameSceneSetup] 플레이 모드에서만 자동 플레이 테스트를 실행할 수 있습니다.");
                return;
            }

            var autoPlay = Object.FindFirstObjectByType<Utilities.AutoPlayTest>();
            if (autoPlay == null)
            {
                var go = new GameObject("AutoPlayTest");
                autoPlay = go.AddComponent<Utilities.AutoPlayTest>();
            }

            autoPlay.StartAutoPlay();
            Debug.Log("[GameSceneSetup] 자동 플레이 테스트 시작!");
        }

        [MenuItem("GoldenAge/Test/Stop Auto Play Test")]
        public static void StopAutoPlayTest()
        {
            var autoPlay = Object.FindFirstObjectByType<Utilities.AutoPlayTest>();
            if (autoPlay != null)
            {
                autoPlay.StopAutoPlay();
                Debug.Log("[GameSceneSetup] 자동 플레이 테스트 중지!");
            }
        }

        [MenuItem("GoldenAge/Scene/Create Complete Game Scene")]
        public static void CreateCompleteGameScene()
        {
            // 기본 씬 구조 생성
            CreateSceneHierarchy();

            // 매니저 추가
            AddAllGameSystems();

            // 라이팅 설정
            SetupLighting();

            // 포스트 프로세싱 설정
            SetupPostProcessing();

            Debug.Log("[GameSceneSetup] 완전한 게임 씬 구조 생성 완료!");
        }

        private static void CreateSceneHierarchy()
        {
            // 메인 카테고리 오브젝트 생성
            string[] categories = {
                "GameSystems",
                "Environment",
                "Player",
                "NPCs",
                "Enemies",
                "Interactables",
                "VFX",
                "Audio",
                "Lighting",
                "UI",
                "DebugSystems"
            };

            foreach (var category in categories)
            {
                if (GameObject.Find(category) == null)
                {
                    new GameObject(category);
                }
            }

            // 메인 카메라 설정
            var mainCamera = Camera.main;
            if (mainCamera == null)
            {
                var cameraGo = new GameObject("Main Camera");
                mainCamera = cameraGo.AddComponent<Camera>();
                mainCamera.tag = "MainCamera";
                cameraGo.AddComponent<AudioListener>();
            }

            // EventSystem
            if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                var eventSystem = new GameObject("EventSystem");
                eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
                eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }

            // Canvas
            var canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                var canvasGo = new GameObject("Canvas");
                canvas = canvasGo.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasGo.AddComponent<UnityEngine.UI.CanvasScaler>();
                canvasGo.AddComponent<UnityEngine.UI.GraphicRaycaster>();
                canvasGo.transform.SetParent(GameObject.Find("UI")?.transform);
            }
        }

        private static void SetupLighting()
        {
            var lightingParent = GameObject.Find("Lighting");
            if (lightingParent == null)
            {
                lightingParent = new GameObject("Lighting");
            }

            // Directional Light
            var directionalLight = GameObject.Find("Directional Light");
            if (directionalLight == null)
            {
                directionalLight = new GameObject("Directional Light");
                var light = directionalLight.AddComponent<Light>();
                light.type = LightType.Directional;
                light.color = new Color(1f, 0.95f, 0.85f);
                light.intensity = 1f;
                light.shadows = LightShadows.Soft;
                directionalLight.transform.rotation = Quaternion.Euler(50, -30, 0);
                directionalLight.transform.SetParent(lightingParent.transform);
            }

            // 환경 조명 설정
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.3f, 0.35f, 0.4f);
            RenderSettings.ambientEquatorColor = new Color(0.2f, 0.2f, 0.25f);
            RenderSettings.ambientGroundColor = new Color(0.1f, 0.1f, 0.12f);
        }

        private static void SetupPostProcessing()
        {
            // URP의 경우 Volume 추가
            var postProcessVolume = GameObject.Find("PostProcessVolume");
            if (postProcessVolume == null)
            {
                postProcessVolume = new GameObject("PostProcessVolume");

                var lightingParent = GameObject.Find("Lighting");
                if (lightingParent != null)
                {
                    postProcessVolume.transform.SetParent(lightingParent.transform);
                }

                // Volume 컴포넌트 추가
#if UNITY_2019_3_OR_NEWER
                var volume = postProcessVolume.AddComponent<UnityEngine.Rendering.Volume>();
                volume.isGlobal = true;
#endif
            }
        }

        private static void AddComponentIfMissing<T>(GameObject go) where T : Component
        {
            if (go.GetComponent<T>() == null)
            {
                go.AddComponent<T>();
            }
        }
    }
}
