using UnityEngine;
using System;
using System.Collections.Generic;
using GoldenAge.Core;
using GoldenAge.Player;

namespace GoldenAge.Quest
{
    /// <summary>
    /// 퀘스트 시스템 매니저
    /// </summary>
    public class QuestManager : Singleton<QuestManager>
    {
        [Header("퀘스트 데이터")]
        [SerializeField] private QuestData[] allQuests;

        private Dictionary<string, QuestProgress> activeQuests = new Dictionary<string, QuestProgress>();
        private HashSet<string> completedQuests = new HashSet<string>();

        // Events
        public event Action<QuestData> OnQuestStarted;
        public event Action<QuestData> OnQuestCompleted;
        public event Action<QuestObjective> OnObjectiveUpdated;
        public event Action<QuestObjective> OnObjectiveCompleted;

        /// <summary>
        /// 퀘스트 시작
        /// </summary>
        public void StartQuest(string questID)
        {
            QuestData quest = FindQuestData(questID);
            if (quest == null)
            {
                Debug.LogError($"[QuestManager] Quest not found: {questID}");
                return;
            }

            if (activeQuests.ContainsKey(questID) || completedQuests.Contains(questID))
            {
                Debug.Log($"[QuestManager] Quest already active or completed: {questID}");
                return;
            }

            QuestProgress progress = new QuestProgress(quest);
            activeQuests.Add(questID, progress);

            OnQuestStarted?.Invoke(quest);
            Debug.Log($"[QuestManager] Quest started: {quest.questName}");

            // 첫 번째 목표 로그
            if (progress.CurrentObjective != null)
            {
                Debug.Log($"[QuestManager] Objective: {progress.CurrentObjective.description}");
            }
        }

        /// <summary>
        /// 퀘스트 목표 업데이트
        /// </summary>
        public void UpdateObjective(ObjectiveType type, string targetID, int amount = 1)
        {
            List<string> questsToCheck = new List<string>(activeQuests.Keys);

            foreach (string questID in questsToCheck)
            {
                QuestProgress progress = activeQuests[questID];
                if (progress.IsComplete) continue;

                QuestObjective currentObj = progress.CurrentObjective;
                if (currentObj == null) continue;

                // 현재 목표와 일치하는지 확인
                if (currentObj.type == type && currentObj.targetID == targetID)
                {
                    progress.objectiveProgress[progress.currentObjectiveIndex] += amount;

                    OnObjectiveUpdated?.Invoke(currentObj);

                    int currentProgress = progress.objectiveProgress[progress.currentObjectiveIndex];
                    Debug.Log($"[QuestManager] Objective progress: {currentProgress}/{currentObj.requiredCount}");

                    // 목표 달성 체크
                    if (currentProgress >= currentObj.requiredCount)
                    {
                        OnObjectiveCompleted?.Invoke(currentObj);
                        progress.currentObjectiveIndex++;

                        // 모든 목표 완료 체크
                        if (progress.IsComplete)
                        {
                            CompleteQuest(questID);
                        }
                        else
                        {
                            // 다음 목표 로그
                            QuestObjective nextObj = progress.CurrentObjective;
                            if (nextObj != null)
                            {
                                Debug.Log($"[QuestManager] Next objective: {nextObj.description}");
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 퀘스트 완료
        /// </summary>
        private void CompleteQuest(string questID)
        {
            if (!activeQuests.TryGetValue(questID, out QuestProgress progress))
                return;

            QuestData quest = progress.quest;

            // 보상 지급
            PlayerStats stats = FindObjectOfType<PlayerStats>();
            if (stats != null)
            {
                stats.AddExperience(quest.expReward);
            }

            // 아이템 보상 (추후 Inventory 연동)
            if (quest.itemRewards != null)
            {
                foreach (var item in quest.itemRewards)
                {
                    Debug.Log($"[QuestManager] Item reward: {item?.itemName ?? "Unknown"}");
                }
            }

            // 완료 처리
            activeQuests.Remove(questID);
            completedQuests.Add(questID);

            OnQuestCompleted?.Invoke(quest);
            Debug.Log($"[QuestManager] Quest completed: {quest.questName}");

            // 다음 퀘스트 자동 시작
            if (quest.nextQuest != null)
            {
                StartQuest(quest.nextQuest.questID);
            }
        }

        #region Query Methods

        /// <summary>
        /// 활성 퀘스트 가져오기
        /// </summary>
        public QuestProgress GetActiveQuest(string questID)
        {
            activeQuests.TryGetValue(questID, out QuestProgress progress);
            return progress;
        }

        /// <summary>
        /// 현재 목표 가져오기
        /// </summary>
        public QuestObjective GetCurrentObjective(string questID)
        {
            if (activeQuests.TryGetValue(questID, out QuestProgress progress))
            {
                return progress.CurrentObjective;
            }
            return null;
        }

        /// <summary>
        /// 퀘스트 완료 여부 확인
        /// </summary>
        public bool IsQuestCompleted(string questID)
        {
            return completedQuests.Contains(questID);
        }

        /// <summary>
        /// 퀘스트 활성 여부 확인
        /// </summary>
        public bool IsQuestActive(string questID)
        {
            return activeQuests.ContainsKey(questID);
        }

        /// <summary>
        /// 모든 활성 퀘스트 가져오기
        /// </summary>
        public List<QuestProgress> GetAllActiveQuests()
        {
            return new List<QuestProgress>(activeQuests.Values);
        }

        /// <summary>
        /// 완료된 퀘스트 ID 목록
        /// </summary>
        public List<string> GetCompletedQuestIDs()
        {
            return new List<string>(completedQuests);
        }

        #endregion

        #region Save/Load

        /// <summary>
        /// 저장용 데이터 가져오기
        /// </summary>
        public List<QuestSaveData> GetActiveQuestsSaveData()
        {
            List<QuestSaveData> data = new List<QuestSaveData>();

            foreach (var kvp in activeQuests)
            {
                data.Add(new QuestSaveData
                {
                    questID = kvp.Key,
                    currentObjectiveIndex = kvp.Value.currentObjectiveIndex,
                    objectiveProgress = (int[])kvp.Value.objectiveProgress.Clone()
                });
            }

            return data;
        }

        /// <summary>
        /// 퀘스트 데이터 불러오기
        /// </summary>
        public void LoadQuestData(List<QuestSaveData> active, List<string> completed)
        {
            activeQuests.Clear();
            completedQuests.Clear();

            // 완료된 퀘스트
            if (completed != null)
            {
                foreach (var questID in completed)
                {
                    completedQuests.Add(questID);
                }
            }

            // 활성 퀘스트
            if (active != null)
            {
                foreach (var saveData in active)
                {
                    QuestData questData = FindQuestData(saveData.questID);
                    if (questData != null)
                    {
                        QuestProgress progress = new QuestProgress(questData)
                        {
                            currentObjectiveIndex = saveData.currentObjectiveIndex,
                            objectiveProgress = saveData.objectiveProgress
                        };
                        activeQuests[saveData.questID] = progress;
                    }
                }
            }
        }

        #endregion

        private QuestData FindQuestData(string questID)
        {
            if (allQuests == null) return null;
            return System.Array.Find(allQuests, q => q.questID == questID);
        }
    }

    /// <summary>
    /// 퀘스트 저장 데이터
    /// </summary>
    [System.Serializable]
    public class QuestSaveData
    {
        public string questID;
        public int currentObjectiveIndex;
        public int[] objectiveProgress;
    }
}
