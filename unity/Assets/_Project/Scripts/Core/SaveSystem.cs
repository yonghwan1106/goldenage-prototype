using UnityEngine;
using System;
using System.IO;

namespace GoldenAge.Core
{
    /// <summary>
    /// 게임 저장/불러오기 시스템
    /// </summary>
    public class SaveSystem : Singleton<SaveSystem>
    {
        [Header("설정")]
        [SerializeField] private string saveFileName = "goldenage_save";
        [SerializeField] private bool useEncryption = false;

        private string SavePath => Path.Combine(Application.persistentDataPath, $"{saveFileName}.json");

        public event Action OnSaveComplete;
        public event Action OnLoadComplete;

        /// <summary>
        /// 게임 저장
        /// </summary>
        public void SaveGame()
        {
            try
            {
                SaveData data = CollectSaveData();
                string json = JsonUtility.ToJson(data, true);

                if (useEncryption)
                {
                    json = EncryptDecrypt(json);
                }

                File.WriteAllText(SavePath, json);

                PlayerPrefs.SetString("LastSaveTime", DateTime.Now.ToString());
                PlayerPrefs.SetInt("SaveData", 1);
                PlayerPrefs.Save();

                OnSaveComplete?.Invoke();
                Debug.Log($"[SaveSystem] 게임 저장 완료: {SavePath}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] 저장 실패: {e.Message}");
            }
        }

        /// <summary>
        /// 게임 불러오기
        /// </summary>
        public void LoadGame()
        {
            if (!HasSaveData())
            {
                Debug.LogWarning("[SaveSystem] 저장 데이터가 없습니다.");
                return;
            }

            try
            {
                string json = File.ReadAllText(SavePath);

                if (useEncryption)
                {
                    json = EncryptDecrypt(json);
                }

                SaveData data = JsonUtility.FromJson<SaveData>(json);
                ApplySaveData(data);

                OnLoadComplete?.Invoke();
                Debug.Log("[SaveSystem] 게임 불러오기 완료");
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] 불러오기 실패: {e.Message}");
            }
        }

        /// <summary>
        /// 저장 데이터 존재 여부
        /// </summary>
        public bool HasSaveData()
        {
            return File.Exists(SavePath) && PlayerPrefs.HasKey("SaveData");
        }

        /// <summary>
        /// 저장 데이터 삭제
        /// </summary>
        public void DeleteSaveData()
        {
            if (File.Exists(SavePath))
            {
                File.Delete(SavePath);
            }

            PlayerPrefs.DeleteKey("SaveData");
            PlayerPrefs.DeleteKey("LastSaveTime");
            PlayerPrefs.Save();

            Debug.Log("[SaveSystem] 저장 데이터 삭제됨");
        }

        /// <summary>
        /// 마지막 저장 시간
        /// </summary>
        public string GetLastSaveTime()
        {
            return PlayerPrefs.GetString("LastSaveTime", "없음");
        }

        private SaveData CollectSaveData()
        {
            SaveData data = new SaveData();

            // 플레이어 데이터
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                data.playerPosition = player.transform.position;
                data.playerRotation = player.transform.eulerAngles;

                var stats = player.GetComponent<Player.PlayerStats>();
                if (stats != null)
                {
                    data.currentHealth = stats.CurrentHealth;
                    data.currentEnergy = stats.CurrentEnergy;
                    data.level = stats.Level;
                    data.experience = stats.Experience;
                }
            }

            // 게임 상태
            data.gameState = GameManager.Instance?.CurrentState.ToString() ?? "Playing";
            data.playTime = Time.time; // 실제로는 누적 플레이 시간 관리 필요

            // 퀘스트 상태
            var questManager = Quest.QuestManager.Instance;
            if (questManager != null)
            {
                data.activeQuestIds = questManager.GetActiveQuestIds();
                data.completedQuestIds = questManager.GetCompletedQuestIds();
            }

            // 저장 시간
            data.saveTime = DateTime.Now.ToString();

            return data;
        }

        private void ApplySaveData(SaveData data)
        {
            // 플레이어 데이터 적용
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                player.transform.position = data.playerPosition;
                player.transform.eulerAngles = data.playerRotation;

                var stats = player.GetComponent<Player.PlayerStats>();
                if (stats != null)
                {
                    stats.SetHealth(data.currentHealth);
                    stats.SetEnergy(data.currentEnergy);
                    stats.SetLevel(data.level);
                    stats.SetExperience(data.experience);
                }
            }

            // 퀘스트 상태 복원
            var questManager = Quest.QuestManager.Instance;
            if (questManager != null && data.activeQuestIds != null)
            {
                questManager.RestoreQuestState(data.activeQuestIds, data.completedQuestIds);
            }
        }

        // 간단한 XOR 암호화 (프로덕션에서는 더 강력한 암호화 필요)
        private string EncryptDecrypt(string data)
        {
            string key = "GoldenAge1920";
            char[] result = new char[data.Length];

            for (int i = 0; i < data.Length; i++)
            {
                result[i] = (char)(data[i] ^ key[i % key.Length]);
            }

            return new string(result);
        }
    }

    [Serializable]
    public class SaveData
    {
        // 플레이어
        public Vector3 playerPosition;
        public Vector3 playerRotation;
        public float currentHealth;
        public float currentEnergy;
        public int level;
        public int experience;

        // 인벤토리 (간략화)
        public string[] inventoryItemIds;
        public int[] inventoryItemCounts;

        // 퀘스트
        public string[] activeQuestIds;
        public string[] completedQuestIds;

        // 게임 상태
        public string gameState;
        public float playTime;

        // 메타
        public string saveTime;
        public string gameVersion;
    }
}
