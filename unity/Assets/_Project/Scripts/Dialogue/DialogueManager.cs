using UnityEngine;
using System.Collections.Generic;
using GoldenAge.Core;

namespace GoldenAge.Dialogue
{
    /// <summary>
    /// 대화 시스템 매니저
    /// </summary>
    public class DialogueManager : Singleton<DialogueManager>
    {
        [Header("UI 참조")]
        [SerializeField] private DialogueUI dialogueUI;

        private DialogueData currentDialogue;
        private int currentLineIndex;
        private bool isDialogueActive;

        // 플래그 저장소
        private HashSet<string> gameFlags = new HashSet<string>();

        // Properties
        public bool IsDialogueActive => isDialogueActive;
        public DialogueData CurrentDialogue => currentDialogue;
        public DialogueLine CurrentLine => GetCurrentLine();

        // Events
        public event System.Action OnDialogueStarted;
        public event System.Action OnDialogueEnded;
        public event System.Action<DialogueLine> OnLineDisplayed;
        public event System.Action<DialogueChoice> OnChoiceSelected;
        public event System.Action<string> OnFlagSet;

        /// <summary>
        /// 대화 시작
        /// </summary>
        public void StartDialogue(DialogueData dialogue)
        {
            if (dialogue == null || isDialogueActive) return;

            // 조건 체크
            if (!string.IsNullOrEmpty(dialogue.requiredFlag) && !HasFlag(dialogue.requiredFlag))
            {
                Debug.Log($"[DialogueManager] Required flag not met: {dialogue.requiredFlag}");
                return;
            }

            isDialogueActive = true;
            currentDialogue = dialogue;
            currentLineIndex = 0;

            GameManager.Instance?.ChangeState(GameState.Dialogue);

            if (dialogueUI != null)
            {
                dialogueUI.Show();
            }

            OnDialogueStarted?.Invoke();
            DisplayCurrentLine();

            Debug.Log($"[DialogueManager] Started dialogue: {dialogue.dialogueID}");
        }

        /// <summary>
        /// 현재 대사 표시
        /// </summary>
        private void DisplayCurrentLine()
        {
            DialogueLine line = GetCurrentLine();

            if (line == null)
            {
                EndDialogue();
                return;
            }

            if (dialogueUI != null)
            {
                dialogueUI.DisplayLine(line);
            }

            // 음성 재생
            if (line.voiceLine != null)
            {
                AudioManager.Instance?.PlayVoice(line.voiceLine);
            }

            OnLineDisplayed?.Invoke(line);
        }

        /// <summary>
        /// 다음 대사로 진행
        /// </summary>
        public void NextLine()
        {
            if (!isDialogueActive) return;

            DialogueLine currentLine = GetCurrentLine();

            // 선택지가 있으면 선택 대기
            if (currentLine != null && currentLine.HasChoices)
            {
                if (dialogueUI != null)
                {
                    dialogueUI.ShowChoices(currentLine.choices);
                }
                return;
            }

            // 다음 라인으로
            currentLineIndex++;
            DisplayCurrentLine();
        }

        /// <summary>
        /// 선택지 선택
        /// </summary>
        public void SelectChoice(int choiceIndex)
        {
            if (!isDialogueActive) return;

            DialogueLine currentLine = GetCurrentLine();
            if (currentLine == null || !currentLine.HasChoices) return;

            if (choiceIndex < 0 || choiceIndex >= currentLine.choices.Length) return;

            DialogueChoice choice = currentLine.choices[choiceIndex];

            // 플래그 설정
            if (!string.IsNullOrEmpty(choice.setFlag))
            {
                SetFlag(choice.setFlag);
            }

            // 퀘스트 시작
            if (!string.IsNullOrEmpty(choice.startQuest))
            {
                Quest.QuestManager.Instance?.StartQuest(choice.startQuest);
            }

            // 호감도 변화 (추후 구현)
            if (choice.relationshipChange != 0)
            {
                Debug.Log($"[DialogueManager] Relationship changed by {choice.relationshipChange}");
            }

            OnChoiceSelected?.Invoke(choice);

            // 다음 대화로 전환 또는 종료
            if (choice.nextDialogue != null)
            {
                currentDialogue = choice.nextDialogue;
                currentLineIndex = 0;
                DisplayCurrentLine();
            }
            else
            {
                EndDialogue();
            }
        }

        /// <summary>
        /// 대화 종료
        /// </summary>
        public void EndDialogue()
        {
            if (!isDialogueActive) return;

            isDialogueActive = false;
            currentDialogue = null;
            currentLineIndex = 0;

            // 음성 중지
            AudioManager.Instance?.StopVoice();

            if (dialogueUI != null)
            {
                dialogueUI.Hide();
            }

            GameManager.Instance?.ChangeState(GameState.Exploration);
            OnDialogueEnded?.Invoke();

            Debug.Log("[DialogueManager] Dialogue ended");
        }

        /// <summary>
        /// 대화 강제 종료 (ESC 등)
        /// </summary>
        public void ForceEndDialogue()
        {
            if (isDialogueActive)
            {
                EndDialogue();
            }
        }

        private DialogueLine GetCurrentLine()
        {
            if (currentDialogue == null || currentDialogue.lines == null)
                return null;

            if (currentLineIndex >= currentDialogue.lines.Length)
                return null;

            return currentDialogue.lines[currentLineIndex];
        }

        #region Flag System

        /// <summary>
        /// 플래그 설정
        /// </summary>
        public void SetFlag(string flag)
        {
            if (string.IsNullOrEmpty(flag)) return;

            if (gameFlags.Add(flag))
            {
                OnFlagSet?.Invoke(flag);
                Debug.Log($"[DialogueManager] Flag set: {flag}");
            }
        }

        /// <summary>
        /// 플래그 보유 여부 확인
        /// </summary>
        public bool HasFlag(string flag)
        {
            return gameFlags.Contains(flag);
        }

        /// <summary>
        /// 플래그 제거
        /// </summary>
        public void RemoveFlag(string flag)
        {
            gameFlags.Remove(flag);
        }

        /// <summary>
        /// 모든 플래그 가져오기 (저장용)
        /// </summary>
        public List<string> GetAllFlags()
        {
            return new List<string>(gameFlags);
        }

        /// <summary>
        /// 플래그 일괄 설정 (불러오기용)
        /// </summary>
        public void SetAllFlags(List<string> flags)
        {
            gameFlags.Clear();
            if (flags != null)
            {
                foreach (var flag in flags)
                {
                    gameFlags.Add(flag);
                }
            }
        }

        #endregion
    }
}
