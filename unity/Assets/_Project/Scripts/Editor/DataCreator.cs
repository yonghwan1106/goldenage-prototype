using UnityEngine;
using UnityEditor;
using GoldenAge.Combat;

namespace GoldenAge.Editor
{
    /// <summary>
    /// ScriptableObject 데이터 생성 유틸리티
    /// </summary>
    public class DataCreator : EditorWindow
    {
        [MenuItem("GoldenAge/Data Creator")]
        public static void ShowWindow()
        {
            GetWindow<DataCreator>("Data Creator");
        }

        private void OnGUI()
        {
            GUILayout.Label("GoldenAge 데이터 생성 도구", EditorStyles.boldLabel);
            GUILayout.Space(10);

            if (GUILayout.Button("모든 기본 데이터 생성"))
            {
                CreateAllDefaultData();
            }

            GUILayout.Space(20);
            GUILayout.Label("개별 생성", EditorStyles.boldLabel);

            if (GUILayout.Button("플레이어 데이터 생성"))
            {
                CreatePlayerData();
            }

            if (GUILayout.Button("적 데이터 생성"))
            {
                CreateEnemyData();
            }

            if (GUILayout.Button("스킬 데이터 생성"))
            {
                CreateSkillData();
            }

            if (GUILayout.Button("아이템 데이터 생성"))
            {
                CreateItemData();
            }
        }

        [MenuItem("GoldenAge/Create All Default Data")]
        public static void CreateAllDefaultData()
        {
            CreatePlayerData();
            CreateEnemyData();
            CreateSkillData();
            CreateItemData();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[GoldenAge] 모든 기본 데이터 생성 완료!");
        }

        private static void CreatePlayerData()
        {
            string path = "Assets/_Project/Data/Characters/";
            EnsureDirectory(path);

            // 플레이어 캐릭터 데이터
            var playerData = ScriptableObject.CreateInstance<CharacterData>();
            playerData.characterName = "탐정 주인공";
            playerData.maxHealth = 100f;
            playerData.maxEnergy = 100f;
            playerData.moveSpeed = 5f;
            playerData.sprintMultiplier = 1.5f;
            playerData.attackPower = 15f;
            playerData.defense = 5f;

            AssetDatabase.CreateAsset(playerData, path + "PlayerCharacterData.asset");
            Debug.Log("[GoldenAge] 플레이어 데이터 생성됨");
        }

        private static void CreateEnemyData()
        {
            string path = "Assets/_Project/Data/Characters/";
            EnsureDirectory(path);

            // 일반 갱스터
            var gangster = ScriptableObject.CreateInstance<CharacterData>();
            gangster.characterName = "갱스터";
            gangster.maxHealth = 50f;
            gangster.maxEnergy = 0f;
            gangster.moveSpeed = 3.5f;
            gangster.attackPower = 10f;
            gangster.defense = 2f;
            AssetDatabase.CreateAsset(gangster, path + "Enemy_Gangster.asset");

            // 정예 갱스터
            var eliteGangster = ScriptableObject.CreateInstance<CharacterData>();
            eliteGangster.characterName = "정예 갱스터";
            eliteGangster.maxHealth = 80f;
            eliteGangster.maxEnergy = 0f;
            eliteGangster.moveSpeed = 4f;
            eliteGangster.attackPower = 15f;
            eliteGangster.defense = 5f;
            AssetDatabase.CreateAsset(eliteGangster, path + "Enemy_EliteGangster.asset");

            // 보스: 빅 토니
            var boss = ScriptableObject.CreateInstance<CharacterData>();
            boss.characterName = "빅 토니";
            boss.maxHealth = 200f;
            boss.maxEnergy = 100f;
            boss.moveSpeed = 3f;
            boss.attackPower = 25f;
            boss.defense = 10f;
            AssetDatabase.CreateAsset(boss, path + "Enemy_BigTony.asset");

            Debug.Log("[GoldenAge] 적 데이터 3개 생성됨");
        }

        private static void CreateSkillData()
        {
            string path = "Assets/_Project/Data/Skills/";
            EnsureDirectory(path);

            // 테슬라 충격기
            var teslaShock = ScriptableObject.CreateInstance<AttackData>();
            teslaShock.attackName = "테슬라 충격기";
            teslaShock.baseDamage = 25f;
            teslaShock.damageType = DamageType.Electric;
            teslaShock.range = 8f;
            teslaShock.cooldown = 3f;
            teslaShock.energyCost = 20f;
            teslaShock.canApplyStatus = true;
            teslaShock.statusType = StatusEffectType.Shocked;
            teslaShock.statusDuration = 2f;
            teslaShock.statusChance = 0.5f;
            AssetDatabase.CreateAsset(teslaShock, path + "Skill_TeslaShock.asset");

            // 에테르 파동
            var etherWave = ScriptableObject.CreateInstance<AttackData>();
            etherWave.attackName = "에테르 파동";
            etherWave.baseDamage = 20f;
            etherWave.damageType = DamageType.Ether;
            etherWave.range = 6f;
            etherWave.cooldown = 4f;
            etherWave.energyCost = 25f;
            etherWave.canApplyStatus = true;
            etherWave.statusType = StatusEffectType.Weakened;
            etherWave.statusDuration = 3f;
            etherWave.statusChance = 0.6f;
            AssetDatabase.CreateAsset(etherWave, path + "Skill_EtherWave.asset");

            // 근접 공격
            var meleeAttack = ScriptableObject.CreateInstance<AttackData>();
            meleeAttack.attackName = "근접 공격";
            meleeAttack.baseDamage = 15f;
            meleeAttack.damageType = DamageType.Physical;
            meleeAttack.range = 2f;
            meleeAttack.cooldown = 0.5f;
            meleeAttack.energyCost = 0f;
            AssetDatabase.CreateAsset(meleeAttack, path + "Skill_MeleeAttack.asset");

            // 차원 전격 (융합 콤보)
            var fusionBlast = ScriptableObject.CreateInstance<AttackData>();
            fusionBlast.attackName = "차원 전격";
            fusionBlast.baseDamage = 60f;
            fusionBlast.damageType = DamageType.Fusion;
            fusionBlast.range = 10f;
            fusionBlast.cooldown = 0f; // 콤보로만 발동
            fusionBlast.energyCost = 0f;
            fusionBlast.canApplyStatus = true;
            fusionBlast.statusType = StatusEffectType.Stunned;
            fusionBlast.statusDuration = 2f;
            fusionBlast.statusChance = 1f;
            AssetDatabase.CreateAsset(fusionBlast, path + "Skill_FusionBlast.asset");

            Debug.Log("[GoldenAge] 스킬 데이터 4개 생성됨");
        }

        private static void CreateItemData()
        {
            string path = "Assets/_Project/Data/Items/";
            EnsureDirectory(path);

            // 체력 포션
            var healthPotion = ScriptableObject.CreateInstance<ItemData>();
            healthPotion.itemName = "응급 치료 키트";
            healthPotion.description = "체력을 30 회복합니다.";
            healthPotion.itemType = ItemType.Consumable;
            healthPotion.rarity = ItemRarity.Common;
            healthPotion.healAmount = 30f;
            healthPotion.maxStack = 5;
            healthPotion.buyPrice = 50;
            healthPotion.sellPrice = 25;
            AssetDatabase.CreateAsset(healthPotion, path + "Item_HealthKit.asset");

            // 에너지 포션
            var energyPotion = ScriptableObject.CreateInstance<ItemData>();
            energyPotion.itemName = "에테르 바이알";
            energyPotion.description = "에너지를 25 회복합니다.";
            energyPotion.itemType = ItemType.Consumable;
            energyPotion.rarity = ItemRarity.Common;
            energyPotion.energyAmount = 25f;
            energyPotion.maxStack = 5;
            energyPotion.buyPrice = 40;
            energyPotion.sellPrice = 20;
            AssetDatabase.CreateAsset(energyPotion, path + "Item_EtherVial.asset");

            // 단서 아이템
            var clue = ScriptableObject.CreateInstance<ItemData>();
            clue.itemName = "의문의 편지";
            clue.description = "부두에서 발견한 편지. 암호화된 내용이 적혀있다.";
            clue.itemType = ItemType.Quest;
            clue.rarity = ItemRarity.Rare;
            clue.maxStack = 1;
            AssetDatabase.CreateAsset(clue, path + "Item_MysteriousLetter.asset");

            Debug.Log("[GoldenAge] 아이템 데이터 3개 생성됨");
        }

        private static void EnsureDirectory(string path)
        {
            if (!AssetDatabase.IsValidFolder(path.TrimEnd('/')))
            {
                string[] folders = path.Split('/');
                string currentPath = folders[0];

                for (int i = 1; i < folders.Length; i++)
                {
                    if (string.IsNullOrEmpty(folders[i])) continue;

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
}
