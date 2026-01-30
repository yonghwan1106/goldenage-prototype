using UnityEngine;
using UnityEditor;
using GoldenAge.Player;
using GoldenAge.Combat;
using GoldenAge.NPC;

namespace GoldenAge.Editor
{
    /// <summary>
    /// 프리팹 자동 생성 도구
    /// </summary>
    public class PrefabCreator : EditorWindow
    {
        private string prefabName = "NewPrefab";
        private PrefabType prefabType = PrefabType.Player;

        [MenuItem("GoldenAge/Prefab Creator")]
        public static void ShowWindow()
        {
            GetWindow<PrefabCreator>("Prefab Creator");
        }

        private void OnGUI()
        {
            GUILayout.Label("GoldenAge 프리팹 생성 도구", EditorStyles.boldLabel);
            GUILayout.Space(10);

            prefabName = EditorGUILayout.TextField("프리팹 이름", prefabName);
            prefabType = (PrefabType)EditorGUILayout.EnumPopup("프리팹 타입", prefabType);

            GUILayout.Space(10);

            if (GUILayout.Button("프리팹 생성"))
            {
                CreatePrefab();
            }

            GUILayout.Space(20);
            GUILayout.Label("빠른 생성", EditorStyles.boldLabel);

            if (GUILayout.Button("플레이어 프리팹 생성"))
            {
                CreatePlayerPrefab();
            }

            if (GUILayout.Button("적 프리팹 생성"))
            {
                CreateEnemyPrefab();
            }

            if (GUILayout.Button("NPC 프리팹 생성"))
            {
                CreateNPCPrefab();
            }

            if (GUILayout.Button("픽업 아이템 프리팹 생성"))
            {
                CreatePickupPrefab();
            }
        }

        private void CreatePrefab()
        {
            switch (prefabType)
            {
                case PrefabType.Player:
                    CreatePlayerPrefab(prefabName);
                    break;
                case PrefabType.Enemy:
                    CreateEnemyPrefab(prefabName);
                    break;
                case PrefabType.NPC:
                    CreateNPCPrefab(prefabName);
                    break;
                case PrefabType.Pickup:
                    CreatePickupPrefab(prefabName);
                    break;
            }
        }

        [MenuItem("GoldenAge/Create Player Prefab")]
        public static void CreatePlayerPrefab()
        {
            CreatePlayerPrefab("Player");
        }

        public static void CreatePlayerPrefab(string name)
        {
            // 루트 오브젝트
            GameObject player = new GameObject(name);
            player.tag = "Player";
            player.layer = LayerMask.NameToLayer("Player");

            // CharacterController
            CharacterController cc = player.AddComponent<CharacterController>();
            cc.height = 1.8f;
            cc.radius = 0.3f;
            cc.center = new Vector3(0, 0.9f, 0);

            // 컴포넌트 추가
            player.AddComponent<PlayerMovement>();
            player.AddComponent<PlayerStats>();
            player.AddComponent<PlayerCombat>();
            player.AddComponent<PlayerAnimator>();
            player.AddComponent<InteractionSystem>();

            // 모델 자리 (임시 캡슐)
            GameObject model = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            model.name = "Model";
            model.transform.SetParent(player.transform);
            model.transform.localPosition = new Vector3(0, 1, 0);
            model.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            DestroyImmediate(model.GetComponent<CapsuleCollider>());

            // 공격 히트박스
            GameObject attackPoint = new GameObject("AttackPoint");
            attackPoint.transform.SetParent(player.transform);
            attackPoint.transform.localPosition = new Vector3(0, 1, 1);

            // 프리팹 저장
            SavePrefab(player, "Player");
        }

        [MenuItem("GoldenAge/Create Enemy Prefab")]
        public static void CreateEnemyPrefab()
        {
            CreateEnemyPrefab("Enemy_Gangster");
        }

        public static void CreateEnemyPrefab(string name)
        {
            GameObject enemy = new GameObject(name);
            enemy.tag = "Enemy";
            enemy.layer = LayerMask.NameToLayer("Enemy");

            // Collider
            CapsuleCollider col = enemy.AddComponent<CapsuleCollider>();
            col.height = 1.8f;
            col.radius = 0.3f;
            col.center = new Vector3(0, 0.9f, 0);

            // Rigidbody
            Rigidbody rb = enemy.AddComponent<Rigidbody>();
            rb.isKinematic = true;

            // NavMeshAgent
            UnityEngine.AI.NavMeshAgent agent = enemy.AddComponent<UnityEngine.AI.NavMeshAgent>();
            agent.speed = 3.5f;
            agent.angularSpeed = 120f;
            agent.stoppingDistance = 1.5f;

            // EnemyAI
            enemy.AddComponent<EnemyAI>();

            // Animator (빈 컴포넌트)
            enemy.AddComponent<Animator>();

            // 모델 자리 (임시 캡슐)
            GameObject model = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            model.name = "Model";
            model.transform.SetParent(enemy.transform);
            model.transform.localPosition = new Vector3(0, 1, 0);
            model.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            DestroyImmediate(model.GetComponent<CapsuleCollider>());

            // 빨간색 머티리얼 (적 구분)
            Renderer renderer = model.GetComponent<Renderer>();
            if (renderer != null)
            {
                Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                mat.color = new Color(0.8f, 0.2f, 0.2f);
                renderer.sharedMaterial = mat;
            }

            SavePrefab(enemy, "Enemies");
        }

        [MenuItem("GoldenAge/Create NPC Prefab")]
        public static void CreateNPCPrefab()
        {
            CreateNPCPrefab("NPC_Civilian");
        }

        public static void CreateNPCPrefab(string name)
        {
            GameObject npc = new GameObject(name);
            npc.tag = "NPC";
            npc.layer = LayerMask.NameToLayer("NPC");

            // Collider
            CapsuleCollider col = npc.AddComponent<CapsuleCollider>();
            col.height = 1.8f;
            col.radius = 0.3f;
            col.center = new Vector3(0, 0.9f, 0);
            col.isTrigger = true; // 상호작용용

            // NPC Controller
            npc.AddComponent<NPCController>();
            npc.AddComponent<Animator>();

            // 모델 자리
            GameObject model = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            model.name = "Model";
            model.transform.SetParent(npc.transform);
            model.transform.localPosition = new Vector3(0, 1, 0);
            model.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            DestroyImmediate(model.GetComponent<CapsuleCollider>());

            // 초록색 머티리얼 (NPC 구분)
            Renderer renderer = model.GetComponent<Renderer>();
            if (renderer != null)
            {
                Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                mat.color = new Color(0.2f, 0.8f, 0.3f);
                renderer.sharedMaterial = mat;
            }

            // 상호작용 표시
            GameObject indicator = new GameObject("InteractionIndicator");
            indicator.transform.SetParent(npc.transform);
            indicator.transform.localPosition = new Vector3(0, 2.2f, 0);

            SavePrefab(npc, "NPCs");
        }

        public static void CreatePickupPrefab(string name = "Pickup_Item")
        {
            GameObject pickup = new GameObject(name);
            pickup.tag = "Pickup";
            pickup.layer = LayerMask.NameToLayer("Interactable");

            // Collider (트리거)
            SphereCollider col = pickup.AddComponent<SphereCollider>();
            col.radius = 0.5f;
            col.isTrigger = true;

            // PickupItem 컴포넌트
            pickup.AddComponent<Environment.PickupItem>();

            // 모델 자리 (임시 큐브)
            GameObject model = GameObject.CreatePrimitive(PrimitiveType.Cube);
            model.name = "Model";
            model.transform.SetParent(pickup.transform);
            model.transform.localPosition = Vector3.zero;
            model.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
            DestroyImmediate(model.GetComponent<BoxCollider>());

            // 노란색 머티리얼
            Renderer renderer = model.GetComponent<Renderer>();
            if (renderer != null)
            {
                Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                mat.color = Color.yellow;
                renderer.sharedMaterial = mat;
            }

            SavePrefab(pickup, "Items");
        }

        private static void SavePrefab(GameObject obj, string subfolder)
        {
            string basePath = "Assets/_Project/Prefabs";
            string fullPath = $"{basePath}/{subfolder}";

            // 폴더 생성
            if (!AssetDatabase.IsValidFolder(fullPath))
            {
                if (!AssetDatabase.IsValidFolder(basePath))
                {
                    AssetDatabase.CreateFolder("Assets/_Project", "Prefabs");
                }
                AssetDatabase.CreateFolder(basePath, subfolder);
            }

            // 프리팹 저장
            string prefabPath = $"{fullPath}/{obj.name}.prefab";
            prefabPath = AssetDatabase.GenerateUniqueAssetPath(prefabPath);

            PrefabUtility.SaveAsPrefabAsset(obj, prefabPath);
            DestroyImmediate(obj);

            Debug.Log($"[PrefabCreator] 프리팹 생성됨: {prefabPath}");
            AssetDatabase.Refresh();
        }
    }

    public enum PrefabType
    {
        Player,
        Enemy,
        NPC,
        Pickup
    }
}
