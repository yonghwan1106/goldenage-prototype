using UnityEngine;
using UnityEditor;

namespace GoldenAge.Editor
{
    /// <summary>
    /// 테스트용 프록시 머티리얼 생성 도구
    /// </summary>
    public class MaterialCreator : EditorWindow
    {
        [MenuItem("GoldenAge/Material Creator")]
        public static void ShowWindow()
        {
            GetWindow<MaterialCreator>("Material Creator");
        }

        private void OnGUI()
        {
            GUILayout.Label("GoldenAge 머티리얼 생성 도구", EditorStyles.boldLabel);
            GUILayout.Space(10);

            GUILayout.Label("테스트용 머티리얼을 생성합니다.", EditorStyles.helpBox);
            GUILayout.Label("실제 에셋 수집 전 프로토타입 테스트용입니다.");

            GUILayout.Space(20);

            if (GUILayout.Button("모든 기본 머티리얼 생성", GUILayout.Height(30)))
            {
                CreateAllMaterials();
            }

            GUILayout.Space(20);
            GUILayout.Label("개별 생성", EditorStyles.boldLabel);

            if (GUILayout.Button("환경 머티리얼 생성"))
            {
                CreateEnvironmentMaterials();
            }

            if (GUILayout.Button("캐릭터 머티리얼 생성"))
            {
                CreateCharacterMaterials();
            }

            if (GUILayout.Button("이펙트 머티리얼 생성"))
            {
                CreateEffectMaterials();
            }

            if (GUILayout.Button("UI 머티리얼 생성"))
            {
                CreateUIMaterials();
            }
        }

        [MenuItem("GoldenAge/Create All Proxy Materials")]
        public static void CreateAllMaterials()
        {
            CreateEnvironmentMaterials();
            CreateCharacterMaterials();
            CreateEffectMaterials();
            CreateUIMaterials();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[MaterialCreator] 모든 프록시 머티리얼 생성 완료!");
        }

        private static void CreateEnvironmentMaterials()
        {
            string path = "Assets/_Project/Materials/Environment";
            EnsureDirectory(path);

            // 1920년대 뉴욕 분위기의 색상들
            CreateMaterial(path, "M_Ground_Cobblestone", new Color(0.35f, 0.32f, 0.28f));
            CreateMaterial(path, "M_Ground_Concrete", new Color(0.5f, 0.5f, 0.48f));
            CreateMaterial(path, "M_Ground_Wood", new Color(0.45f, 0.3f, 0.2f));

            CreateMaterial(path, "M_Wall_Brick_Red", new Color(0.55f, 0.25f, 0.2f));
            CreateMaterial(path, "M_Wall_Brick_Brown", new Color(0.4f, 0.28f, 0.2f));
            CreateMaterial(path, "M_Wall_Concrete", new Color(0.6f, 0.58f, 0.55f));

            CreateMaterial(path, "M_Metal_Rusty", new Color(0.45f, 0.3f, 0.25f));
            CreateMaterial(path, "M_Metal_Dark", new Color(0.2f, 0.2f, 0.22f));
            CreateMaterial(path, "M_Metal_Brass", new Color(0.7f, 0.55f, 0.3f));

            CreateMaterial(path, "M_Water", new Color(0.2f, 0.35f, 0.4f), 0.8f);

            Debug.Log("[MaterialCreator] 환경 머티리얼 생성됨");
        }

        private static void CreateCharacterMaterials()
        {
            string path = "Assets/_Project/Materials/Characters";
            EnsureDirectory(path);

            // 플레이어
            CreateMaterial(path, "M_Player_Suit", new Color(0.15f, 0.15f, 0.18f)); // 어두운 정장
            CreateMaterial(path, "M_Player_Shirt", new Color(0.9f, 0.88f, 0.85f)); // 흰 셔츠
            CreateMaterial(path, "M_Player_Skin", new Color(0.85f, 0.7f, 0.6f));

            // 적 (갱스터)
            CreateMaterial(path, "M_Enemy_Suit", new Color(0.25f, 0.22f, 0.2f));
            CreateMaterial(path, "M_Enemy_Hat", new Color(0.2f, 0.18f, 0.15f));

            // NPC
            CreateMaterial(path, "M_NPC_Civilian", new Color(0.4f, 0.35f, 0.3f));
            CreateMaterial(path, "M_NPC_Worker", new Color(0.35f, 0.3f, 0.25f));

            Debug.Log("[MaterialCreator] 캐릭터 머티리얼 생성됨");
        }

        private static void CreateEffectMaterials()
        {
            string path = "Assets/_Project/Materials/Effects";
            EnsureDirectory(path);

            // 테슬라 (전기)
            CreateMaterial(path, "M_Effect_Tesla", new Color(0.3f, 0.7f, 1f), 0.5f, true);
            CreateMaterial(path, "M_Effect_Electric_Spark", new Color(0.8f, 0.95f, 1f), 0.8f, true);

            // 에테르 (보라색 마법)
            CreateMaterial(path, "M_Effect_Ether", new Color(0.6f, 0.3f, 0.9f), 0.6f, true);
            CreateMaterial(path, "M_Effect_Ether_Glow", new Color(0.8f, 0.5f, 1f), 0.9f, true);

            // 융합 (차원 전격)
            CreateMaterial(path, "M_Effect_Fusion", new Color(1f, 0.8f, 0.3f), 0.7f, true);
            CreateMaterial(path, "M_Effect_Fusion_Core", new Color(1f, 0.95f, 0.8f), 0.95f, true);

            // 피격
            CreateMaterial(path, "M_Effect_Hit", new Color(1f, 0.3f, 0.2f), 0.5f, true);

            Debug.Log("[MaterialCreator] 이펙트 머티리얼 생성됨");
        }

        private static void CreateUIMaterials()
        {
            string path = "Assets/_Project/Materials/UI";
            EnsureDirectory(path);

            // 1920년대 아르데코 스타일 색상
            CreateMaterial(path, "M_UI_Gold", new Color(0.85f, 0.7f, 0.35f));
            CreateMaterial(path, "M_UI_Burgundy", new Color(0.5f, 0.15f, 0.2f));
            CreateMaterial(path, "M_UI_Navy", new Color(0.1f, 0.15f, 0.25f));
            CreateMaterial(path, "M_UI_Cream", new Color(0.95f, 0.92f, 0.85f));
            CreateMaterial(path, "M_UI_Black", new Color(0.1f, 0.1f, 0.12f));

            Debug.Log("[MaterialCreator] UI 머티리얼 생성됨");
        }

        private static void CreateMaterial(string path, string name, Color color, float alpha = 1f, bool emissive = false)
        {
            // URP Lit 셰이더 사용
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }

            Material mat = new Material(shader);
            mat.color = new Color(color.r, color.g, color.b, alpha);

            // 투명도 설정
            if (alpha < 1f)
            {
                mat.SetFloat("_Surface", 1); // Transparent
                mat.SetFloat("_Blend", 0); // Alpha
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.SetInt("_ZWrite", 0);
                mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                mat.renderQueue = 3000;
            }

            // 이미시브 설정
            if (emissive)
            {
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", color * 2f);
            }

            // 저장
            string fullPath = $"{path}/{name}.mat";
            AssetDatabase.CreateAsset(mat, fullPath);
        }

        private static void EnsureDirectory(string path)
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
