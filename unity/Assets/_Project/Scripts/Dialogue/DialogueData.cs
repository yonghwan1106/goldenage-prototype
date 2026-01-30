using UnityEngine;

namespace GoldenAge.Dialogue
{
    /// <summary>
    /// 대화 데이터 ScriptableObject
    /// </summary>
    [CreateAssetMenu(fileName = "NewDialogue", menuName = "GoldenAge/Dialogue Data")]
    public class DialogueData : ScriptableObject
    {
        [Header("기본 정보")]
        public string dialogueID;
        public string dialogueTitle;    // 에디터용

        [Header("대화 내용")]
        public DialogueLine[] lines;

        [Header("조건")]
        public string requiredFlag;     // 이 플래그가 있어야 대화 가능
        public string requiredQuest;    // 이 퀘스트가 활성화되어 있어야 대화 가능
    }

    /// <summary>
    /// 대화 한 줄
    /// </summary>
    [System.Serializable]
    public class DialogueLine
    {
        [Header("화자")]
        public string speakerName;
        public Sprite speakerPortrait;

        [Header("내용")]
        [TextArea(3, 5)]
        public string text;

        [Header("음성 (선택)")]
        public AudioClip voiceLine;

        [Header("선택지 (비어있으면 자동 진행)")]
        public DialogueChoice[] choices;

        /// <summary>
        /// 선택지가 있는지 확인
        /// </summary>
        public bool HasChoices => choices != null && choices.Length > 0;
    }

    /// <summary>
    /// 대화 선택지
    /// </summary>
    [System.Serializable]
    public class DialogueChoice
    {
        [Header("선택지 텍스트")]
        public string choiceText;

        [Header("다음 대화")]
        public DialogueData nextDialogue;

        [Header("결과")]
        public string setFlag;          // 선택 시 설정할 플래그
        public string startQuest;       // 선택 시 시작할 퀘스트
        public int relationshipChange;  // 호감도 변화 (-10 ~ +10)
    }
}
