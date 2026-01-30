using UnityEngine;
using System;
using System.Collections.Generic;

namespace GoldenAge.Core
{
    /// <summary>
    /// 업적/도전과제 시스템
    /// </summary>
    public class AchievementSystem : Singleton<AchievementSystem>
    {
        [Header("업적 데이터")]
        [SerializeField] private AchievementData[] achievements;

        [Header("알림")]
        [SerializeField] private bool showNotifications = true;
        [SerializeField] private AudioClip unlockSound;

        private Dictionary<string, AchievementProgress> progressMap = new Dictionary<string, AchievementProgress>();
        private HashSet<string> unlockedAchievements = new HashSet<string>();

        public event Action<AchievementData> OnAchievementUnlocked;
        public event Action<AchievementData, float> OnAchievementProgress;

        protected override void Awake()
        {
            base.Awake();
            InitializeAchievements();
            LoadProgress();
        }

        private void InitializeAchievements()
        {
            if (achievements == null) return;

            foreach (var achievement in achievements)
            {
                if (!progressMap.ContainsKey(achievement.id))
                {
                    progressMap[achievement.id] = new AchievementProgress
                    {
                        id = achievement.id,
                        currentValue = 0,
                        isUnlocked = false
                    };
                }
            }
        }

        /// <summary>
        /// 진행도 업데이트
        /// </summary>
        public void UpdateProgress(string achievementId, float value, bool additive = true)
        {
            if (!progressMap.TryGetValue(achievementId, out var progress))
            {
                Debug.LogWarning($"[Achievement] 알 수 없는 업적: {achievementId}");
                return;
            }

            if (progress.isUnlocked) return;

            AchievementData data = GetAchievementData(achievementId);
            if (data == null) return;

            // 값 업데이트
            if (additive)
                progress.currentValue += value;
            else
                progress.currentValue = value;

            // 진행도 이벤트
            float percent = Mathf.Clamp01(progress.currentValue / data.targetValue);
            OnAchievementProgress?.Invoke(data, percent);

            // 완료 체크
            if (progress.currentValue >= data.targetValue)
            {
                UnlockAchievement(achievementId);
            }

            SaveProgress();
        }

        /// <summary>
        /// 업적 잠금 해제
        /// </summary>
        public void UnlockAchievement(string achievementId)
        {
            if (unlockedAchievements.Contains(achievementId)) return;

            if (!progressMap.TryGetValue(achievementId, out var progress)) return;

            progress.isUnlocked = true;
            progress.unlockTime = DateTime.Now;
            unlockedAchievements.Add(achievementId);

            AchievementData data = GetAchievementData(achievementId);
            if (data != null)
            {
                // 이벤트
                OnAchievementUnlocked?.Invoke(data);

                // 알림
                if (showNotifications)
                {
                    UI.NotificationSystem.Instance?.Show(
                        $"업적 달성: {data.title}",
                        UI.NotificationType.Success,
                        4f
                    );
                }

                // 사운드
                if (unlockSound != null)
                {
                    AudioSource.PlayClipAtPoint(unlockSound, Camera.main.transform.position);
                }

                // 보상
                GrantReward(data);

                Debug.Log($"[Achievement] 업적 달성: {data.title}");
            }

            SaveProgress();
        }

        private void GrantReward(AchievementData data)
        {
            if (data.rewardExp > 0)
            {
                var player = GameObject.FindGameObjectWithTag("Player");
                var stats = player?.GetComponent<Player.PlayerStats>();
                stats?.AddExperience(data.rewardExp);
            }

            // 추가 보상 처리 (아이템 등)
        }

        /// <summary>
        /// 업적 데이터 가져오기
        /// </summary>
        public AchievementData GetAchievementData(string id)
        {
            if (achievements == null) return null;

            foreach (var a in achievements)
            {
                if (a.id == id) return a;
            }
            return null;
        }

        /// <summary>
        /// 업적 잠금 해제 여부
        /// </summary>
        public bool IsUnlocked(string achievementId)
        {
            return unlockedAchievements.Contains(achievementId);
        }

        /// <summary>
        /// 진행도 가져오기
        /// </summary>
        public float GetProgress(string achievementId)
        {
            if (!progressMap.TryGetValue(achievementId, out var progress)) return 0f;

            AchievementData data = GetAchievementData(achievementId);
            if (data == null) return 0f;

            return Mathf.Clamp01(progress.currentValue / data.targetValue);
        }

        /// <summary>
        /// 모든 업적 목록
        /// </summary>
        public AchievementData[] GetAllAchievements()
        {
            return achievements;
        }

        /// <summary>
        /// 해금된 업적 수
        /// </summary>
        public int GetUnlockedCount()
        {
            return unlockedAchievements.Count;
        }

        /// <summary>
        /// 총 업적 수
        /// </summary>
        public int GetTotalCount()
        {
            return achievements?.Length ?? 0;
        }

        #region Save/Load

        private void SaveProgress()
        {
            var saveData = new AchievementSaveData
            {
                progressList = new List<AchievementProgress>(progressMap.Values)
            };

            string json = JsonUtility.ToJson(saveData);
            PlayerPrefs.SetString("Achievements", json);
            PlayerPrefs.Save();
        }

        private void LoadProgress()
        {
            string json = PlayerPrefs.GetString("Achievements", "");
            if (string.IsNullOrEmpty(json)) return;

            try
            {
                var saveData = JsonUtility.FromJson<AchievementSaveData>(json);
                if (saveData?.progressList != null)
                {
                    foreach (var progress in saveData.progressList)
                    {
                        progressMap[progress.id] = progress;
                        if (progress.isUnlocked)
                        {
                            unlockedAchievements.Add(progress.id);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Achievement] 로드 실패: {e.Message}");
            }
        }

        /// <summary>
        /// 진행도 초기화
        /// </summary>
        public void ResetAllProgress()
        {
            progressMap.Clear();
            unlockedAchievements.Clear();
            InitializeAchievements();
            PlayerPrefs.DeleteKey("Achievements");
            Debug.Log("[Achievement] 모든 진행도 초기화됨");
        }

        #endregion
    }

    [Serializable]
    public class AchievementData
    {
        public string id;
        public string title;
        [TextArea] public string description;
        public Sprite icon;
        public AchievementType type;
        public float targetValue = 1;
        public bool isHidden = false;

        [Header("보상")]
        public int rewardExp;
        public string rewardItemId;
    }

    public enum AchievementType
    {
        Kill,           // 적 처치
        Quest,          // 퀘스트 완료
        Collect,        // 아이템 수집
        Explore,        // 장소 발견
        Combo,          // 콤보 달성
        Level,          // 레벨 달성
        Time,           // 플레이 시간
        Special         // 특수
    }

    [Serializable]
    public class AchievementProgress
    {
        public string id;
        public float currentValue;
        public bool isUnlocked;
        public DateTime unlockTime;
    }

    [Serializable]
    public class AchievementSaveData
    {
        public List<AchievementProgress> progressList;
    }
}
