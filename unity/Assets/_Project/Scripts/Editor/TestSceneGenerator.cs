using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace GoldenAge.Editor
{
    /// <summary>
    /// 테스트 씬 자동 생성 도구
    /// </summary>
    public class TestSceneGenerator : EditorWindow
    {
        private string sceneName = "TestScene";
        private bool createGround = true;
        private bool createPlayer = true;
        private bool createEnemies = true;
        private bool createLighting = true;
        private bool createUI = true;
        private bool createManagers = true;

        private int enemyCount = 3;
        private float groundSize = 50f;

        [MenuItem("GoldenAge/Test Scene Generator")]
        public static void ShowWindow()
        {
            GetWindow<TestSceneGenerator>("Test Scene Generator");
        }

        private void OnGUI()
        {
            GUILayout.Label("테스트 씬 생성 도구", EditorStyles.boldLabel);
            GUILayout.Space(10);

            sceneName = EditorGUILayout.TextField("씬 이름", sceneName);

            GUILayout.Space(10);
            GUILayout.Label("포함 요소", EditorStyles.boldLabel);

            createGround = EditorGUILayout.Toggle("바닥 생성", createGround);
            if (createGround)
            {
                EditorGUI.indentLevel++;
                groundSize = EditorGUILayout.FloatField("바닥 크기", groundSize);
                EditorGUI.indentLevel--;
            }

            createPlayer = EditorGUILayout.Toggle("플레이어 생성", createPlayer);
            createManagers = EditorGUILayout.Toggle("매니저 생성", createManagers);
            createLighting = EditorGUILayout.Toggle("조명 생성", createLighting);
            createUI = EditorGUILayout.Toggle("UI 생성", createUI);

            createEnemies = EditorGUILayout.Toggle("적 생성", createEnemies);
            if (createEnemies)
            {
                EditorGUI.indentLevel++;
                enemyCount = EditorGUILayout.IntSlider("적 수", enemyCount, 1, 10);
                EditorGUI.indentLevel--;
            }

            GUILayout.Space(20);

            if (GUILayout.Button("테스트 씬 생성", GUILayout.Height(40)))
            {
                GenerateTestScene();
            }

            GUILayout.Space(10);

            if (GUILayout.Button("현재 씬에 테스트 요소 추가"))
            {
                AddTestElementsToCurrentScene();
            }
        }

        [MenuItem("GoldenAge/Quick Test Scene")]
        public static void QuickTestScene()
        {
            var generator = CreateInstance<TestSceneGenerator>();
            generator.GenerateTestScene();
        }

        private void GenerateTestScene()
        {
            // 새 씬 생성
            Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            AddTestElementsToCurrentScene();

            // 씬 저장
            string path = $"Assets/_Project/Scenes/Test/{sceneName}.unity";
            EnsureDirectory("Assets/_Project/Scenes/Test");

            EditorSceneManager.SaveScene(newScene, path);
            Debug.Log($"[TestSceneGenerator] 테스트 씬 생성됨: {path}");
        }

        private void AddTestElementsToCurrentScene()
        {
            // 루트 오브젝트 생성
            if (createManagers) CreateManagers();
            if (createGround) CreateGround();
            if (createLighting) CreateLighting();
            if (createPlayer) CreatePlayer();
            if (createEnemies) CreateEnemies();
            if (createUI) CreateUI();

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Debug.Log("[TestSceneGenerator] 테스트 요소 추가됨");
        }

        private void CreateManagers()
        {
            GameObject managersRoot = new GameObject("--- MANAGERS ---");

            // GameManager
            GameObject gmObj = new GameObject("GameManager");
            gmObj.transform.SetParent(managersRoot.transform);
            AddComponentIfExists(gmObj, "GoldenAge.Core.GameManager");

            // AudioManager
            GameObject amObj = new GameObject("AudioManager");
            amObj.transform.SetParent(managersRoot.transform);
            AddComponentIfExists(amObj, "GoldenAge.Core.AudioManager");

            // VFXManager
            GameObject vfxObj = new GameObject("VFXManager");
            vfxObj.transform.SetParent(managersRoot.transform);
            AddComponentIfExists(vfxObj, "GoldenAge.Core.VFXManager");

            // CombatManager
            GameObject cmObj = new GameObject("CombatManager");
            cmObj.transform.SetParent(managersRoot.transform);
            AddComponentIfExists(cmObj, "GoldenAge.Combat.CombatManager");

            // ObjectPool
            GameObject poolObj = new GameObject("ObjectPool");
            poolObj.transform.SetParent(managersRoot.transform);
            AddComponentIfExists(poolObj, "GoldenAge.Core.ObjectPool");

            // SaveSystem
            GameObject saveObj = new GameObject("SaveSystem");
            saveObj.transform.SetParent(managersRoot.transform);
            AddComponentIfExists(saveObj, "GoldenAge.Core.SaveSystem");

            // GameSettings
            GameObject settingsObj = new GameObject("GameSettings");
            settingsObj.transform.SetParent(managersRoot.transform);
            AddComponentIfExists(settingsObj, "GoldenAge.Core.GameSettings");

            // DebugConsole
            GameObject consoleObj = new GameObject("DebugConsole");
            consoleObj.transform.SetParent(managersRoot.transform);
            AddComponentIfExists(consoleObj, "GoldenAge.Utilities.DebugConsole");

            // PerformanceMonitor
            GameObject perfObj = new GameObject("PerformanceMonitor");
            perfObj.transform.SetParent(managersRoot.transform);
            AddComponentIfExists(perfObj, "GoldenAge.Utilities.PerformanceMonitor");
        }

        private void CreateGround()
        {
            GameObject envRoot = new GameObject("--- ENVIRONMENT ---");

            // 바닥
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.SetParent(envRoot.transform);
            ground.transform.localScale = new Vector3(groundSize / 10f, 1f, groundSize / 10f);
            ground.layer = LayerMask.NameToLayer("Ground");
            ground.tag = "Ground";

            // 머티리얼
            Renderer renderer = ground.GetComponent<Renderer>();
            if (renderer != null)
            {
                Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                mat.color = new Color(0.3f, 0.3f, 0.3f);
                renderer.sharedMaterial = mat;
            }

            // 벽 (경계)
            CreateWall(envRoot.transform, "Wall_North", new Vector3(0, 2.5f, groundSize / 2f), new Vector3(groundSize, 5f, 1f));
            CreateWall(envRoot.transform, "Wall_South", new Vector3(0, 2.5f, -groundSize / 2f), new Vector3(groundSize, 5f, 1f));
            CreateWall(envRoot.transform, "Wall_East", new Vector3(groundSize / 2f, 2.5f, 0), new Vector3(1f, 5f, groundSize));
            CreateWall(envRoot.transform, "Wall_West", new Vector3(-groundSize / 2f, 2.5f, 0), new Vector3(1f, 5f, groundSize));

            // 장애물
            CreateObstacle(envRoot.transform, "Obstacle_1", new Vector3(5f, 0.5f, 5f));
            CreateObstacle(envRoot.transform, "Obstacle_2", new Vector3(-5f, 0.5f, -5f));
            CreateObstacle(envRoot.transform, "Obstacle_3", new Vector3(8f, 0.5f, -3f));
        }

        private void CreateWall(Transform parent, string name, Vector3 position, Vector3 scale)
        {
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = name;
            wall.transform.SetParent(parent);
            wall.transform.position = position;
            wall.transform.localScale = scale;
            wall.layer = LayerMask.NameToLayer("Obstacle");

            Renderer renderer = wall.GetComponent<Renderer>();
            if (renderer != null)
            {
                Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                mat.color = new Color(0.4f, 0.35f, 0.3f);
                renderer.sharedMaterial = mat;
            }
        }

        private void CreateObstacle(Transform parent, string name, Vector3 position)
        {
            GameObject obstacle = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obstacle.name = name;
            obstacle.transform.SetParent(parent);
            obstacle.transform.position = position;
            obstacle.transform.localScale = new Vector3(2f, 1f, 2f);
            obstacle.layer = LayerMask.NameToLayer("Obstacle");

            Renderer renderer = obstacle.GetComponent<Renderer>();
            if (renderer != null)
            {
                Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                mat.color = new Color(0.5f, 0.4f, 0.3f);
                renderer.sharedMaterial = mat;
            }
        }

        private void CreateLighting()
        {
            // Directional Light
            GameObject lightObj = new GameObject("Directional Light");
            Light light = lightObj.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = new Color(1f, 0.95f, 0.85f);
            light.intensity = 1f;
            light.shadows = LightShadows.Soft;
            lightObj.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            // 앰비언트 라이트 설정
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.3f, 0.35f, 0.4f);
        }

        private void CreatePlayer()
        {
            GameObject charsRoot = new GameObject("--- CHARACTERS ---");

            // 플레이어
            GameObject player = new GameObject("Player");
            player.tag = "Player";
            player.layer = LayerMask.NameToLayer("Player");
            player.transform.SetParent(charsRoot.transform);
            player.transform.position = new Vector3(0, 0, 0);

            // CharacterController
            CharacterController cc = player.AddComponent<CharacterController>();
            cc.height = 1.8f;
            cc.radius = 0.3f;
            cc.center = new Vector3(0, 0.9f, 0);

            // 컴포넌트 추가
            AddComponentIfExists(player, "GoldenAge.Player.PlayerMovement");
            AddComponentIfExists(player, "GoldenAge.Player.PlayerStats");
            AddComponentIfExists(player, "GoldenAge.Player.PlayerCombat");
            AddComponentIfExists(player, "GoldenAge.Player.PlayerAnimator");
            AddComponentIfExists(player, "GoldenAge.Player.InteractionSystem");

            // 시각적 표현 (임시 캡슐)
            GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            visual.name = "Visual";
            visual.transform.SetParent(player.transform);
            visual.transform.localPosition = new Vector3(0, 1f, 0);
            visual.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            DestroyImmediate(visual.GetComponent<CapsuleCollider>());

            Renderer renderer = visual.GetComponent<Renderer>();
            if (renderer != null)
            {
                Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                mat.color = new Color(0.2f, 0.5f, 0.8f);
                renderer.sharedMaterial = mat;
            }

            // 카메라
            GameObject camObj = new GameObject("Main Camera");
            camObj.tag = "MainCamera";
            Camera cam = camObj.AddComponent<Camera>();
            camObj.AddComponent<AudioListener>();
            AddComponentIfExists(camObj, "GoldenAge.Core.CameraController");
            AddComponentIfExists(camObj, "GoldenAge.Core.CameraShake");

            camObj.transform.position = new Vector3(0, 5f, -7f);
            camObj.transform.rotation = Quaternion.Euler(25f, 0, 0);
        }

        private void CreateEnemies()
        {
            GameObject enemiesContainer = GameObject.Find("--- CHARACTERS ---")?.transform.Find("Enemies")?.gameObject;
            if (enemiesContainer == null)
            {
                Transform charsRoot = GameObject.Find("--- CHARACTERS ---")?.transform;
                if (charsRoot == null)
                {
                    charsRoot = new GameObject("--- CHARACTERS ---").transform;
                }
                enemiesContainer = new GameObject("Enemies");
                enemiesContainer.transform.SetParent(charsRoot);
            }

            for (int i = 0; i < enemyCount; i++)
            {
                float angle = (360f / enemyCount) * i * Mathf.Deg2Rad;
                float radius = 10f;
                Vector3 pos = new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);

                CreateEnemy(enemiesContainer.transform, $"Enemy_{i + 1}", pos);
            }
        }

        private void CreateEnemy(Transform parent, string name, Vector3 position)
        {
            GameObject enemy = new GameObject(name);
            enemy.tag = "Enemy";
            enemy.layer = LayerMask.NameToLayer("Enemy");
            enemy.transform.SetParent(parent);
            enemy.transform.position = position;

            // Collider
            CapsuleCollider col = enemy.AddComponent<CapsuleCollider>();
            col.height = 1.8f;
            col.radius = 0.3f;
            col.center = new Vector3(0, 0.9f, 0);

            // NavMeshAgent
            enemy.AddComponent<UnityEngine.AI.NavMeshAgent>();

            // EnemyAI
            AddComponentIfExists(enemy, "GoldenAge.Combat.EnemyAI");

            // 시각적 표현 (빨간 캡슐)
            GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            visual.name = "Visual";
            visual.transform.SetParent(enemy.transform);
            visual.transform.localPosition = new Vector3(0, 1f, 0);
            visual.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            DestroyImmediate(visual.GetComponent<CapsuleCollider>());

            Renderer renderer = visual.GetComponent<Renderer>();
            if (renderer != null)
            {
                Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                mat.color = new Color(0.8f, 0.2f, 0.2f);
                renderer.sharedMaterial = mat;
            }
        }

        private void CreateUI()
        {
            GameObject uiRoot = new GameObject("--- UI ---");

            // Canvas
            GameObject canvasObj = new GameObject("Canvas_HUD");
            canvasObj.transform.SetParent(uiRoot.transform);

            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();

            // EventSystem
            if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                GameObject eventSystem = new GameObject("EventSystem");
                eventSystem.transform.SetParent(uiRoot.transform);
                eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
                eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }
        }

        private void AddComponentIfExists(GameObject obj, string typeName)
        {
            System.Type type = System.Type.GetType(typeName + ", Assembly-CSharp");
            if (type != null)
            {
                obj.AddComponent(type);
            }
            else
            {
                Debug.LogWarning($"[TestSceneGenerator] 타입을 찾을 수 없음: {typeName}");
            }
        }

        private void EnsureDirectory(string path)
        {
            string[] folders = path.Split('/');
            string currentPath = folders[0];

            for (int i = 1; i < folders.Length; i++)
            {
                string newPath = currentPath + "/" + folders[i];
                if (!AssetDatabase.IsValidFolder(newPath))
                {
                    AssetDatabase.CreateFolder(currentPath, folders[i]);
                }
                currentPath = newPath;
            }
        }
    }
}
