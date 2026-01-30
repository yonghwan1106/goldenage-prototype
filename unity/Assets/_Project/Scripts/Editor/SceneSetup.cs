using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace GoldenAge.Editor
{
    /// <summary>
    /// 씬 기본 구조 설정 에디터 도구
    /// </summary>
    public class SceneSetup : EditorWindow
    {
        [MenuItem("GoldenAge/Scene Setup")]
        public static void ShowWindow()
        {
            GetWindow<SceneSetup>("Scene Setup");
        }

        private void OnGUI()
        {
            GUILayout.Label("GoldenAge 씬 설정 도구", EditorStyles.boldLabel);
            GUILayout.Space(10);

            if (GUILayout.Button("기본 게임 씬 구조 생성"))
            {
                CreateGameSceneStructure();
            }

            GUILayout.Space(10);

            if (GUILayout.Button("메인 메뉴 씬 구조 생성"))
            {
                CreateMenuSceneStructure();
            }

            GUILayout.Space(20);
            GUILayout.Label("개별 오브젝트 생성", EditorStyles.boldLabel);

            if (GUILayout.Button("매니저 오브젝트 생성"))
            {
                CreateManagers();
            }

            if (GUILayout.Button("플레이어 스폰 포인트 생성"))
            {
                CreateSpawnPoint();
            }

            if (GUILayout.Button("적 스폰 포인트 생성"))
            {
                CreateEnemySpawnPoint();
            }
        }

        [MenuItem("GoldenAge/Setup Game Scene Structure")]
        public static void CreateGameSceneStructure()
        {
            // 루트 오브젝트들 생성
            CreateOrGetRoot("--- MANAGERS ---");
            CreateOrGetRoot("--- ENVIRONMENT ---");
            CreateOrGetRoot("--- CHARACTERS ---");
            CreateOrGetRoot("--- UI ---");
            CreateOrGetRoot("--- AUDIO ---");
            CreateOrGetRoot("--- VFX ---");

            // 매니저들
            GameObject managers = CreateOrGetRoot("--- MANAGERS ---");
            CreateManagerChild(managers, "GameManager", "GoldenAge.Core.GameManager");
            CreateManagerChild(managers, "AudioManager", "GoldenAge.Core.AudioManager");
            CreateManagerChild(managers, "VFXManager", "GoldenAge.Core.VFXManager");
            CreateManagerChild(managers, "CombatManager", "GoldenAge.Combat.CombatManager");
            CreateManagerChild(managers, "DialogueManager", "GoldenAge.Dialogue.DialogueManager");
            CreateManagerChild(managers, "QuestManager", "GoldenAge.Quest.QuestManager");
            CreateManagerChild(managers, "DebugCommands", "GoldenAge.Utilities.DebugCommands");

            // 환경
            GameObject environment = CreateOrGetRoot("--- ENVIRONMENT ---");
            CreateChild(environment, "Ground");
            CreateChild(environment, "Buildings");
            CreateChild(environment, "Props");
            CreateChild(environment, "Lights");

            // 캐릭터
            GameObject characters = CreateOrGetRoot("--- CHARACTERS ---");
            CreateChild(characters, "Player");
            CreateChild(characters, "Enemies");
            CreateChild(characters, "NPCs");
            CreateChild(characters, "SpawnPoints");

            // UI
            GameObject ui = CreateOrGetRoot("--- UI ---");
            CreateChild(ui, "Canvas_HUD");
            CreateChild(ui, "Canvas_Dialogue");
            CreateChild(ui, "Canvas_Pause");

            // 오디오
            GameObject audio = CreateOrGetRoot("--- AUDIO ---");
            CreateChild(audio, "BGM_Source");
            CreateChild(audio, "Ambient_Source");

            // 기본 조명
            CreateDirectionalLight();

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Debug.Log("[GoldenAge] 게임 씬 구조 생성 완료!");
        }

        public static void CreateMenuSceneStructure()
        {
            CreateOrGetRoot("--- MANAGERS ---");
            CreateOrGetRoot("--- UI ---");
            CreateOrGetRoot("--- AUDIO ---");
            CreateOrGetRoot("--- CAMERA ---");

            GameObject managers = CreateOrGetRoot("--- MANAGERS ---");
            CreateChild(managers, "MenuManager");

            GameObject ui = CreateOrGetRoot("--- UI ---");
            CreateChild(ui, "Canvas_MainMenu");
            CreateChild(ui, "Canvas_Settings");
            CreateChild(ui, "Canvas_Credits");

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Debug.Log("[GoldenAge] 메뉴 씬 구조 생성 완료!");
        }

        private static GameObject CreateOrGetRoot(string name)
        {
            GameObject obj = GameObject.Find(name);
            if (obj == null)
            {
                obj = new GameObject(name);
                obj.transform.position = Vector3.zero;
            }
            return obj;
        }

        private static GameObject CreateChild(GameObject parent, string name)
        {
            Transform existing = parent.transform.Find(name);
            if (existing != null) return existing.gameObject;

            GameObject child = new GameObject(name);
            child.transform.SetParent(parent.transform);
            child.transform.localPosition = Vector3.zero;
            return child;
        }

        private static void CreateManagerChild(GameObject parent, string name, string componentType)
        {
            GameObject child = CreateChild(parent, name);

            System.Type type = System.Type.GetType(componentType + ", Assembly-CSharp");
            if (type != null && child.GetComponent(type) == null)
            {
                child.AddComponent(type);
            }
        }

        private static void CreateManagers()
        {
            GameObject managers = CreateOrGetRoot("--- MANAGERS ---");
            CreateManagerChild(managers, "GameManager", "GoldenAge.Core.GameManager");
            CreateManagerChild(managers, "AudioManager", "GoldenAge.Core.AudioManager");
            CreateManagerChild(managers, "VFXManager", "GoldenAge.Core.VFXManager");
            CreateManagerChild(managers, "CombatManager", "GoldenAge.Combat.CombatManager");
            CreateManagerChild(managers, "DialogueManager", "GoldenAge.Dialogue.DialogueManager");
            CreateManagerChild(managers, "QuestManager", "GoldenAge.Quest.QuestManager");

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Debug.Log("[GoldenAge] 매니저 오브젝트 생성 완료!");
        }

        private static void CreateSpawnPoint()
        {
            GameObject spawnPoint = new GameObject("PlayerSpawnPoint");
            spawnPoint.tag = "SpawnPoint";
            spawnPoint.transform.position = Vector3.zero;

            // 에디터에서 시각화를 위한 아이콘 설정
            var iconContent = EditorGUIUtility.IconContent("sv_icon_dot3_pix16_gizmo");
            if (iconContent != null && iconContent.image != null)
            {
                // Unity 2021.2+ 버전
                EditorGUIUtility.SetIconForObject(spawnPoint, (Texture2D)iconContent.image);
            }

            Selection.activeGameObject = spawnPoint;
            Debug.Log("[GoldenAge] 플레이어 스폰 포인트 생성됨");
        }

        private static void CreateEnemySpawnPoint()
        {
            GameObject spawnPoint = new GameObject("EnemySpawnPoint");
            spawnPoint.tag = "SpawnPoint";
            spawnPoint.transform.position = new Vector3(5, 0, 5);

            var iconContent = EditorGUIUtility.IconContent("sv_icon_dot6_pix16_gizmo");
            if (iconContent != null && iconContent.image != null)
            {
                EditorGUIUtility.SetIconForObject(spawnPoint, (Texture2D)iconContent.image);
            }

            Selection.activeGameObject = spawnPoint;
            Debug.Log("[GoldenAge] 적 스폰 포인트 생성됨");
        }

        private static void CreateDirectionalLight()
        {
            Light[] lights = Object.FindObjectsOfType<Light>();
            foreach (var light in lights)
            {
                if (light.type == LightType.Directional)
                    return; // 이미 있음
            }

            GameObject lightObj = new GameObject("Directional Light");
            Light directionalLight = lightObj.AddComponent<Light>();
            directionalLight.type = LightType.Directional;
            directionalLight.color = new Color(1f, 0.95f, 0.84f); // 따뜻한 저녁 조명
            directionalLight.intensity = 1f;
            directionalLight.shadows = LightShadows.Soft;

            lightObj.transform.rotation = Quaternion.Euler(50, -30, 0);

            GameObject environment = CreateOrGetRoot("--- ENVIRONMENT ---");
            GameObject lightsParent = CreateChild(environment, "Lights");
            lightObj.transform.SetParent(lightsParent.transform);
        }
    }
}
