using UnityEngine;
using System;
using System.Collections.Generic;

namespace GoldenAge.Core
{
    /// <summary>
    /// 게임 내 분석/통계 시스템
    /// </summary>
    public class AnalyticsSystem : Singleton<AnalyticsSystem>
    {
        [Header("설정")]
        [SerializeField] private bool enableAnalytics = true;
        [SerializeField] private bool logToConsole = true;

        // 세션 데이터
        private DateTime sessionStartTime;
        private float totalPlayTime;
        private int sessionCount;

        // 게임 통계
        private GameStatistics stats = new GameStatistics();

        public GameStatistics Stats => stats;

        protected override void Awake()
        {
            base.Awake();
            LoadStatistics();
            StartSession();
        }

        private void OnApplicationQuit()
        {
            EndSession();
            SaveStatistics();
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                SaveStatistics();
            }
        }

        private void StartSession()
        {
            sessionStartTime = DateTime.Now;
            sessionCount++;
            stats.totalSessions = sessionCount;

            LogEvent("session_start", new Dictionary<string, object>
            {
                { "session_number", sessionCount },
                { "platform", Application.platform.ToString() }
            });
        }

        private void EndSession()
        {
            TimeSpan sessionDuration = DateTime.Now - sessionStartTime;
            stats.totalPlayTimeSeconds += (float)sessionDuration.TotalSeconds;

            LogEvent("session_end", new Dictionary<string, object>
            {
                { "duration_seconds", sessionDuration.TotalSeconds },
                { "total_play_time", stats.totalPlayTimeSeconds }
            });
        }

        /// <summary>
        /// 이벤트 로깅
        /// </summary>
        public void LogEvent(string eventName, Dictionary<string, object> parameters = null)
        {
            if (!enableAnalytics) return;

            if (logToConsole)
            {
                string paramStr = parameters != null
                    ? string.Join(", ", System.Linq.Enumerable.Select(parameters, p => $"{p.Key}={p.Value}"))
                    : "";
                Debug.Log($"[Analytics] {eventName}: {paramStr}");
            }

            // 외부 분석 서비스 연동 가능
            // Unity Analytics, Firebase Analytics 등
        }

        #region Game Events

        /// <summary>
        /// 적 처치 기록
        /// </summary>
        public void LogEnemyKill(string enemyType, string weaponUsed)
        {
            stats.totalEnemiesKilled++;

            if (!stats.enemyKillsByType.ContainsKey(enemyType))
                stats.enemyKillsByType[enemyType] = 0;
            stats.enemyKillsByType[enemyType]++;

            LogEvent("enemy_kill", new Dictionary<string, object>
            {
                { "enemy_type", enemyType },
                { "weapon", weaponUsed },
                { "total_kills", stats.totalEnemiesKilled }
            });
        }

        /// <summary>
        /// 플레이어 사망 기록
        /// </summary>
        public void LogPlayerDeath(string cause, string location)
        {
            stats.totalDeaths++;

            LogEvent("player_death", new Dictionary<string, object>
            {
                { "cause", cause },
                { "location", location },
                { "total_deaths", stats.totalDeaths }
            });
        }

        /// <summary>
        /// 퀘스트 완료 기록
        /// </summary>
        public void LogQuestComplete(string questId, float completionTime)
        {
            stats.totalQuestsCompleted++;

            LogEvent("quest_complete", new Dictionary<string, object>
            {
                { "quest_id", questId },
                { "completion_time", completionTime },
                { "total_quests", stats.totalQuestsCompleted }
            });
        }

        /// <summary>
        /// 아이템 획득 기록
        /// </summary>
        public void LogItemCollected(string itemId, string itemType)
        {
            stats.totalItemsCollected++;

            LogEvent("item_collected", new Dictionary<string, object>
            {
                { "item_id", itemId },
                { "item_type", itemType },
                { "total_items", stats.totalItemsCollected }
            });
        }

        /// <summary>
        /// 레벨업 기록
        /// </summary>
        public void LogLevelUp(int newLevel, float playTime)
        {
            if (newLevel > stats.highestLevel)
                stats.highestLevel = newLevel;

            LogEvent("level_up", new Dictionary<string, object>
            {
                { "new_level", newLevel },
                { "play_time", playTime }
            });
        }

        /// <summary>
        /// 업적 달성 기록
        /// </summary>
        public void LogAchievementUnlocked(string achievementId)
        {
            stats.totalAchievements++;

            LogEvent("achievement_unlocked", new Dictionary<string, object>
            {
                { "achievement_id", achievementId },
                { "total_achievements", stats.totalAchievements }
            });
        }

        /// <summary>
        /// 스킬 사용 기록
        /// </summary>
        public void LogSkillUsed(string skillName)
        {
            if (!stats.skillUsageCount.ContainsKey(skillName))
                stats.skillUsageCount[skillName] = 0;
            stats.skillUsageCount[skillName]++;

            LogEvent("skill_used", new Dictionary<string, object>
            {
                { "skill_name", skillName },
                { "usage_count", stats.skillUsageCount[skillName] }
            });
        }

        /// <summary>
        /// 데미지 기록
        /// </summary>
        public void LogDamageDealt(float amount, string source)
        {
            stats.totalDamageDealt += amount;

            if (amount > stats.highestDamage)
                stats.highestDamage = amount;
        }

        /// <summary>
        /// 받은 데미지 기록
        /// </summary>
        public void LogDamageTaken(float amount, string source)
        {
            stats.totalDamageTaken += amount;
        }

        #endregion

        #region Save/Load

        private void SaveStatistics()
        {
            string json = JsonUtility.ToJson(stats);
            PlayerPrefs.SetString("GameStatistics", json);
            PlayerPrefs.SetInt("SessionCount", sessionCount);
            PlayerPrefs.Save();
        }

        private void LoadStatistics()
        {
            sessionCount = PlayerPrefs.GetInt("SessionCount", 0);

            string json = PlayerPrefs.GetString("GameStatistics", "");
            if (!string.IsNullOrEmpty(json))
            {
                try
                {
                    stats = JsonUtility.FromJson<GameStatistics>(json);
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[Analytics] 통계 로드 실패: {e.Message}");
                    stats = new GameStatistics();
                }
            }
        }

        /// <summary>
        /// 통계 초기화
        /// </summary>
        public void ResetStatistics()
        {
            stats = new GameStatistics();
            sessionCount = 0;
            PlayerPrefs.DeleteKey("GameStatistics");
            PlayerPrefs.DeleteKey("SessionCount");
            Debug.Log("[Analytics] 통계 초기화됨");
        }

        #endregion
    }

    [Serializable]
    public class GameStatistics
    {
        // 세션
        public int totalSessions;
        public float totalPlayTimeSeconds;

        // 전투
        public int totalEnemiesKilled;
        public int totalDeaths;
        public float totalDamageDealt;
        public float totalDamageTaken;
        public float highestDamage;
        public int highestCombo;

        // 진행
        public int totalQuestsCompleted;
        public int totalItemsCollected;
        public int highestLevel;
        public int totalAchievements;

        // 상세 통계
        public SerializableDictionary<string, int> enemyKillsByType = new SerializableDictionary<string, int>();
        public SerializableDictionary<string, int> skillUsageCount = new SerializableDictionary<string, int>();
    }

    /// <summary>
    /// 직렬화 가능한 Dictionary
    /// </summary>
    [Serializable]
    public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
        [SerializeField] private List<TKey> keys = new List<TKey>();
        [SerializeField] private List<TValue> values = new List<TValue>();

        public void OnBeforeSerialize()
        {
            keys.Clear();
            values.Clear();
            foreach (var pair in this)
            {
                keys.Add(pair.Key);
                values.Add(pair.Value);
            }
        }

        public void OnAfterDeserialize()
        {
            Clear();
            for (int i = 0; i < Mathf.Min(keys.Count, values.Count); i++)
            {
                this[keys[i]] = values[i];
            }
        }
    }
}
