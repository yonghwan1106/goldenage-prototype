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

        [MenuItem("GoldenAge/Scene/Add Wave System")]
        public static void AddWaveSystem()
        {
            // GameSystems에 WaveManager 추가
            var gameSystems = GameObject.Find("GameSystems");
            if (gameSystems == null)
            {
                gameSystems = new GameObject("GameSystems");
            }

            // WaveManager 추가
            var waveManager = gameSystems.GetComponent<Combat.WaveManager>();
            if (waveManager == null)
            {
                waveManager = gameSystems.AddComponent<Combat.WaveManager>();
            }

            // 적 프리팹 자동 연결
            string[] enemyPrefabGuids = AssetDatabase.FindAssets("t:Prefab Enemy", new[] { "Assets/_Project/Prefabs" });
            var enemyPrefabs = new System.Collections.Generic.List<GameObject>();

            foreach (string guid in enemyPrefabGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.Contains("Enemy") && !path.Contains("HealthBar"))
                {
                    GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                    if (prefab != null && prefab.GetComponent<Combat.EnemyAI>() != null)
                    {
                        enemyPrefabs.Add(prefab);
                    }
                }
            }

            // SerializedObject로 프리팹 배열 설정
            if (enemyPrefabs.Count > 0)
            {
                SerializedObject so = new SerializedObject(waveManager);
                SerializedProperty prefabsProp = so.FindProperty("enemyPrefabs");
                prefabsProp.arraySize = enemyPrefabs.Count;
                for (int i = 0; i < enemyPrefabs.Count; i++)
                {
                    prefabsProp.GetArrayElementAtIndex(i).objectReferenceValue = enemyPrefabs[i];
                }
                so.ApplyModifiedProperties();
                Debug.Log($"[GameSceneSetup] {enemyPrefabs.Count}개의 적 프리팹 연결됨");
            }

            // 스폰 포인트 생성
            CreateSpawnPoints();

            // CombatManager 추가
            AddComponentIfMissing<Combat.CombatManager>(gameSystems);

            EditorUtility.SetDirty(gameSystems);
            Debug.Log("[GameSceneSetup] 웨이브 시스템 추가 완료!");
        }

        private static void CreateSpawnPoints()
        {
            var spawnPointsParent = GameObject.Find("SpawnPoints");
            if (spawnPointsParent == null)
            {
                spawnPointsParent = new GameObject("SpawnPoints");
            }

            // 4방향 스폰 포인트 생성
            Vector3[] positions = {
                new Vector3(20, 0, 0),
                new Vector3(-20, 0, 0),
                new Vector3(0, 0, 20),
                new Vector3(0, 0, -20),
                new Vector3(15, 0, 15),
                new Vector3(-15, 0, 15),
                new Vector3(15, 0, -15),
                new Vector3(-15, 0, -15)
            };

            string[] names = { "SpawnPoint_East", "SpawnPoint_West", "SpawnPoint_North", "SpawnPoint_South",
                              "SpawnPoint_NE", "SpawnPoint_NW", "SpawnPoint_SE", "SpawnPoint_SW" };

            var spawnPoints = new System.Collections.Generic.List<Transform>();

            for (int i = 0; i < positions.Length; i++)
            {
                var existing = GameObject.Find(names[i]);
                if (existing == null)
                {
                    var spawnPoint = new GameObject(names[i]);
                    spawnPoint.transform.position = positions[i];
                    spawnPoint.transform.SetParent(spawnPointsParent.transform);
                    spawnPoint.AddComponent<Combat.SpawnPoint>();
                    spawnPoints.Add(spawnPoint.transform);
                }
                else
                {
                    spawnPoints.Add(existing.transform);
                }
            }

            // WaveManager에 스폰 포인트 연결
            var waveManager = Object.FindFirstObjectByType<Combat.WaveManager>();
            if (waveManager != null && spawnPoints.Count > 0)
            {
                SerializedObject so = new SerializedObject(waveManager);
                SerializedProperty spawnPointsProp = so.FindProperty("spawnPoints");
                spawnPointsProp.arraySize = spawnPoints.Count;
                for (int i = 0; i < spawnPoints.Count; i++)
                {
                    spawnPointsProp.GetArrayElementAtIndex(i).objectReferenceValue = spawnPoints[i];
                }
                so.ApplyModifiedProperties();
            }

            Debug.Log($"[GameSceneSetup] {spawnPoints.Count}개의 스폰 포인트 생성됨");
        }

        [MenuItem("GoldenAge/Scene/Setup MainGame Scene")]
        public static void SetupMainGameScene()
        {
            // 1. 기본 씬 구조 생성
            CreateSceneHierarchy();

            // 2. 환경 설정
            CreateGameEnvironment();

            // 3. 플레이어 생성
            CreatePlayer();

            // 4. 시스템 추가
            AddAllGameSystems();

            // 5. 웨이브 시스템 추가
            AddWaveSystem();

            // 6. UI 추가
            AddUISystems();
            AddWaveUI();

            // 7. NavMesh 베이크
            BakeNavMesh();

            // 8. 라이팅
            SetupLighting();

            Debug.Log("[GameSceneSetup] MainGame 씬 설정 완료!");
        }

        private static void CreateGameEnvironment()
        {
            var environment = GameObject.Find("Environment");
            if (environment == null)
            {
                environment = new GameObject("Environment");
            }

            // 바닥 생성
            var ground = GameObject.Find("Ground");
            if (ground == null)
            {
                ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
                ground.name = "Ground";
                ground.transform.localScale = new Vector3(10, 1, 10);  // 100x100 유닛
                ground.transform.SetParent(environment.transform);
                ground.isStatic = true;

                // 머티리얼 설정
                var renderer = ground.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material.color = new Color(0.3f, 0.3f, 0.35f);
                }
            }

            // 경계 벽 생성
            CreateBoundaryWalls(environment.transform);

            Debug.Log("[GameSceneSetup] 게임 환경 생성 완료");
        }

        private static void CreateBoundaryWalls(Transform parent)
        {
            float size = 50f;
            float height = 5f;
            float thickness = 1f;

            Vector3[] positions = {
                new Vector3(0, height/2, size),     // North
                new Vector3(0, height/2, -size),    // South
                new Vector3(size, height/2, 0),     // East
                new Vector3(-size, height/2, 0)     // West
            };

            Vector3[] scales = {
                new Vector3(size * 2, height, thickness),
                new Vector3(size * 2, height, thickness),
                new Vector3(thickness, height, size * 2),
                new Vector3(thickness, height, size * 2)
            };

            string[] names = { "Wall_North", "Wall_South", "Wall_East", "Wall_West" };

            for (int i = 0; i < 4; i++)
            {
                if (GameObject.Find(names[i]) == null)
                {
                    var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    wall.name = names[i];
                    wall.transform.position = positions[i];
                    wall.transform.localScale = scales[i];
                    wall.transform.SetParent(parent);
                    wall.isStatic = true;

                    var renderer = wall.GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        renderer.material.color = new Color(0.4f, 0.35f, 0.3f);
                    }
                }
            }
        }

        private static void CreatePlayer()
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                Debug.Log("[GameSceneSetup] 플레이어가 이미 존재합니다.");
                return;
            }

            // 플레이어 프리팹 찾기
            string[] playerPrefabGuids = AssetDatabase.FindAssets("Player t:Prefab", new[] { "Assets/_Project/Prefabs" });
            GameObject playerPrefab = null;

            foreach (string guid in playerPrefabGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.Contains("Player") && !path.Contains("UI"))
                {
                    playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                    break;
                }
            }

            if (playerPrefab != null)
            {
                player = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab);
                player.transform.position = Vector3.zero;
            }
            else
            {
                // 프리팹이 없으면 기본 플레이어 생성
                player = new GameObject("Player");
                player.tag = "Player";
                player.layer = LayerMask.NameToLayer("Player");

                // 기본 컴포넌트 추가
                var capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                capsule.transform.SetParent(player.transform);
                capsule.transform.localPosition = Vector3.up;

                player.AddComponent<CharacterController>();
                player.AddComponent<Player.PlayerStats>();
                player.AddComponent<Player.PlayerMovement>();
                player.AddComponent<Combat.PlayerCombat>();

                var renderer = capsule.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material.color = new Color(0.2f, 0.5f, 0.8f);
                }
            }

            var playerParent = GameObject.Find("Player");
            if (playerParent != null && playerParent != player)
            {
                player.transform.SetParent(playerParent.transform);
            }

            Debug.Log("[GameSceneSetup] 플레이어 생성 완료");
        }

        private static void AddWaveUI()
        {
            var canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas == null) return;

            var waveUI = GameObject.Find("WaveUI");
            if (waveUI == null)
            {
                waveUI = new GameObject("WaveUI");
                waveUI.transform.SetParent(canvas.transform, false);
                waveUI.AddComponent<UI.WaveUI>();

                var rect = waveUI.AddComponent<RectTransform>();
                rect.anchorMin = new Vector2(0.5f, 1);
                rect.anchorMax = new Vector2(0.5f, 1);
                rect.pivot = new Vector2(0.5f, 1);
                rect.anchoredPosition = new Vector2(0, -10);
                rect.sizeDelta = new Vector2(400, 60);

                // Wave 텍스트 추가
                var waveTextGo = new GameObject("WaveText");
                waveTextGo.transform.SetParent(waveUI.transform, false);
                var waveText = waveTextGo.AddComponent<TMPro.TextMeshProUGUI>();
                waveText.text = "Wave 1";
                waveText.fontSize = 36;
                waveText.alignment = TMPro.TextAlignmentOptions.Center;
                waveText.color = Color.white;

                var textRect = waveTextGo.GetComponent<RectTransform>();
                textRect.anchorMin = Vector2.zero;
                textRect.anchorMax = Vector2.one;
                textRect.sizeDelta = Vector2.zero;

                // WaveUI 컴포넌트에 연결
                SerializedObject so = new SerializedObject(waveUI.GetComponent<UI.WaveUI>());
                so.FindProperty("waveText").objectReferenceValue = waveText;
                so.ApplyModifiedProperties();
            }

            Debug.Log("[GameSceneSetup] Wave UI 추가 완료");
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
