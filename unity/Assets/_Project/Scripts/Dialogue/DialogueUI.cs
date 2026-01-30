using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

namespace GoldenAge.Dialogue
{
    /// <summary>
    /// 대화 UI 컨트롤러
    /// </summary>
    public class DialogueUI : MonoBehaviour
    {
        [Header("UI 요소")]
        [SerializeField] private GameObject dialoguePanel;
        [SerializeField] private Image speakerPortrait;
        [SerializeField] private TMP_Text speakerNameText;
        [SerializeField] private TMP_Text dialogueText;
        [SerializeField] private GameObject continueIndicator;

        [Header("선택지")]
        [SerializeField] private GameObject choicePanel;
        [SerializeField] private ChoiceButton[] choiceButtons;

        [Header("타이핑 효과")]
        [SerializeField] private float typingSpeed = 0.03f;
        [SerializeField] private AudioClip typingSound;
        [SerializeField] private int typingSoundInterval = 3;

        private Coroutine typingCoroutine;
        private bool isTyping;
        private string fullText;
        private int charCount;

        private void Awake()
        {
            Hide();
        }

        private void Update()
        {
            // 클릭/스페이스로 진행
            if (dialoguePanel.activeSelf && !choicePanel.activeSelf)
            {
                if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
                {
                    OnContinuePressed();
                }
            }
        }

        /// <summary>
        /// 대화창 표시
        /// </summary>
        public void Show()
        {
            dialoguePanel.SetActive(true);
            choicePanel.SetActive(false);
        }

        /// <summary>
        /// 대화창 숨기기
        /// </summary>
        public void Hide()
        {
            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
            }

            dialoguePanel.SetActive(false);
            choicePanel.SetActive(false);
        }

        /// <summary>
        /// 대화 줄 표시
        /// </summary>
        public void DisplayLine(DialogueLine line)
        {
            // 화자 정보
            speakerNameText.text = line.speakerName;

            if (speakerPortrait != null)
            {
                if (line.speakerPortrait != null)
                {
                    speakerPortrait.sprite = line.speakerPortrait;
                    speakerPortrait.gameObject.SetActive(true);
                }
                else
                {
                    speakerPortrait.gameObject.SetActive(false);
                }
            }

            // UI 상태
            continueIndicator.SetActive(false);
            choicePanel.SetActive(false);

            // 타이핑 효과
            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
            }
            typingCoroutine = StartCoroutine(TypeText(line.text));
        }

        private IEnumerator TypeText(string text)
        {
            isTyping = true;
            fullText = text;
            dialogueText.text = "";
            charCount = 0;

            foreach (char c in text)
            {
                dialogueText.text += c;
                charCount++;

                // 타이핑 사운드
                if (typingSound != null && charCount % typingSoundInterval == 0)
                {
                    Core.AudioManager.Instance?.PlaySFX(typingSound);
                }

                yield return new WaitForSeconds(typingSpeed);
            }

            isTyping = false;
            continueIndicator.SetActive(true);
        }

        /// <summary>
        /// 계속 버튼/클릭 처리
        /// </summary>
        public void OnContinuePressed()
        {
            if (isTyping)
            {
                // 타이핑 스킵
                if (typingCoroutine != null)
                {
                    StopCoroutine(typingCoroutine);
                }
                dialogueText.text = fullText;
                isTyping = false;
                continueIndicator.SetActive(true);
            }
            else
            {
                // 다음 대사로
                DialogueManager.Instance?.NextLine();
            }
        }

        /// <summary>
        /// 선택지 표시
        /// </summary>
        public void ShowChoices(DialogueChoice[] choices)
        {
            choicePanel.SetActive(true);
            continueIndicator.SetActive(false);

            for (int i = 0; i < choiceButtons.Length; i++)
            {
                if (i < choices.Length)
                {
                    choiceButtons[i].gameObject.SetActive(true);
                    choiceButtons[i].Setup(i, choices[i].choiceText);
                }
                else
                {
                    choiceButtons[i].gameObject.SetActive(false);
                }
            }
        }

        /// <summary>
        /// 선택지 버튼에서 호출
        /// </summary>
        public void OnChoiceButtonClicked(int index)
        {
            DialogueManager.Instance?.SelectChoice(index);
        }
    }
}
