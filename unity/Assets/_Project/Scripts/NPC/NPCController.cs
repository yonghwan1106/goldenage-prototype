using UnityEngine;
using GoldenAge.Combat;
using GoldenAge.Dialogue;

namespace GoldenAge.NPC
{
    /// <summary>
    /// NPC 기본 컨트롤러
    /// </summary>
    public class NPCController : MonoBehaviour, IInteractable
    {
        [Header("NPC 정보")]
        [SerializeField] private string npcName = "NPC";
        [SerializeField] private string npcTitle = "";
        [SerializeField] private NPCType npcType = NPCType.Civilian;

        [Header("대화")]
        [SerializeField] private DialogueData dialogueData;
        [SerializeField] private bool hasQuest = false;

        [Header("시각 표시")]
        [SerializeField] private GameObject interactionIndicator;
        [SerializeField] private GameObject questIndicator;

        [Header("애니메이션")]
        [SerializeField] private Animator animator;
        [SerializeField] private string idleAnimation = "Idle";
        [SerializeField] private string talkAnimation = "Talk";

        private bool isTalking = false;

        public string NPCName => npcName;
        public string NPCTitle => npcTitle;
        public NPCType Type => npcType;
        public bool HasQuest => hasQuest;

        private void Start()
        {
            if (animator == null)
                animator = GetComponent<Animator>();

            UpdateIndicators();
        }

        public string GetInteractionPrompt()
        {
            string prompt = $"대화하기: {npcName}";
            if (!string.IsNullOrEmpty(npcTitle))
                prompt = $"대화하기: {npcName} ({npcTitle})";
            return prompt;
        }

        public void OnInteract(GameObject interactor)
        {
            if (isTalking) return;

            StartConversation();
        }

        private void StartConversation()
        {
            isTalking = true;

            // 플레이어 방향으로 회전
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                Vector3 lookDir = player.transform.position - transform.position;
                lookDir.y = 0;
                if (lookDir != Vector3.zero)
                    transform.rotation = Quaternion.LookRotation(lookDir);
            }

            // 대화 애니메이션
            if (animator != null)
                animator.SetTrigger(talkAnimation);

            // 대화 시작
            if (dialogueData != null)
            {
                DialogueManager.Instance?.StartDialogue(dialogueData);
                DialogueManager.Instance.OnDialogueEnd += OnConversationEnd;
            }
            else
            {
                Debug.LogWarning($"[NPC] {npcName}에 대화 데이터가 없습니다.");
                isTalking = false;
            }
        }

        private void OnConversationEnd()
        {
            isTalking = false;
            DialogueManager.Instance.OnDialogueEnd -= OnConversationEnd;

            if (animator != null)
                animator.SetTrigger(idleAnimation);
        }

        public void SetQuestAvailable(bool available)
        {
            hasQuest = available;
            UpdateIndicators();
        }

        private void UpdateIndicators()
        {
            if (interactionIndicator != null)
                interactionIndicator.SetActive(true);

            if (questIndicator != null)
                questIndicator.SetActive(hasQuest);
        }

        private void OnDrawGizmosSelected()
        {
            // NPC 위치 표시
            Gizmos.color = npcType switch
            {
                NPCType.Civilian => Color.green,
                NPCType.Merchant => Color.yellow,
                NPCType.QuestGiver => Color.cyan,
                NPCType.Enemy => Color.red,
                _ => Color.white
            };

            Gizmos.DrawWireSphere(transform.position + Vector3.up, 0.5f);
        }
    }

    public enum NPCType
    {
        Civilian,       // 일반 시민
        Merchant,       // 상인
        QuestGiver,     // 퀘스트 제공자
        Informant,      // 정보원
        Enemy           // 적대적
    }
}
