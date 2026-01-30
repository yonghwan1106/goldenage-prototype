using UnityEngine;

namespace GoldenAge.Quest
{
    /// <summary>
    /// 퀘스트 목표 타입
    /// </summary>
    public enum ObjectiveType
    {
        TalkToNPC,      // NPC와 대화
        KillEnemy,      // 적 처치
        CollectItem,    // 아이템 수집
        ReachLocation,  // 위치 도달
        Investigate     // 오브젝트 조사
    }

    /// <summary>
    /// 퀘스트 데이터 ScriptableObject
    /// </summary>
    [CreateAssetMenu(fileName = "NewQuest", menuName = "GoldenAge/Quest Data")]
    public class QuestData : ScriptableObject
    {
        [Header("기본 정보")]
        public string questID;
        public string questName;

        [TextArea(3, 5)]
        public string description;

        [Header("목표")]
        public QuestObjective[] objectives;

        [Header("보상")]
        public int expReward;
        public Combat.ItemData[] itemRewards;

        [Header("다음 퀘스트")]
        public QuestData nextQuest;
    }

    /// <summary>
    /// 퀘스트 목표
    /// </summary>
    [System.Serializable]
    public class QuestObjective
    {
        [Header("기본 정보")]
        public string objectiveID;

        [TextArea(2, 3)]
        public string description;

        [Header("조건")]
        public ObjectiveType type;
        public string targetID;         // 대상 ID (적, NPC, 아이템 등)
        public int requiredCount = 1;

        [Header("가이드 (선택)")]
        public string locationHint;     // 위치 힌트 텍스트
        public Vector3 markerPosition;  // 맵 마커 위치
    }

    /// <summary>
    /// 퀘스트 진행 상황 (런타임)
    /// </summary>
    [System.Serializable]
    public class QuestProgress
    {
        public QuestData quest;
        public int currentObjectiveIndex;
        public int[] objectiveProgress;

        public QuestProgress(QuestData questData)
        {
            quest = questData;
            currentObjectiveIndex = 0;
            objectiveProgress = new int[questData.objectives.Length];
        }

        public QuestObjective CurrentObjective
        {
            get
            {
                if (quest == null || quest.objectives == null) return null;
                if (currentObjectiveIndex >= quest.objectives.Length) return null;
                return quest.objectives[currentObjectiveIndex];
            }
        }

        public int CurrentProgress => objectiveProgress.Length > currentObjectiveIndex
            ? objectiveProgress[currentObjectiveIndex] : 0;

        public bool IsComplete => currentObjectiveIndex >= quest.objectives.Length;
    }
}
