using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

namespace GoldenAge.Tutorial
{
    /// <summary>
    /// 튜토리얼 시스템
    /// </summary>
    public class TutorialSystem : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private GameObject tutorialPanel;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private TextMeshProUGUI keyPromptText;
        [SerializeField] private Image highlightImage;
        [SerializeField] private Button skipButton;
        [SerializeField] private Button nextButton;

        [Header("설정")]
        [SerializeField] private float typingSpeed = 0.03f;
        [SerializeField] private bool autoAdvance = false;
        [SerializeField] private float autoAdvanceDelay = 3f;

        [Header("튜토리얼 단계")]
        [SerializeField] private TutorialStep[] tutorialSteps;

        private int currentStepIndex = -1;
        private bool isTyping = false;
        private bool isTutorialActive = false;
        private Coroutine typingCoroutine;

        public bool IsTutorialActive => isTutorialActive;

        public event System.Action OnTutorialStart;
        public event System.Action OnTutorialEnd;
        public event System.Action<int> OnStepComplete;

        private void Start()
        {
            if (tutorialPanel != null)
                tutorialPanel.SetActive(false);

            if (skipButton != null)
                skipButton.onClick.AddListener(SkipTutorial);

            if (nextButton != null)
                nextButton.onClick.AddListener(NextStep);

            // 저장된 튜토리얼 상태 확인
            if (!HasCompletedTutorial())
            {
                StartTutorial();
            }
        }

        private void Update()
        {
            if (!isTutorialActive) return;

            // 현재 단계 완료 조건 확인
            if (currentStepIndex >= 0 && currentStepIndex < tutorialSteps.Length)
            {
                TutorialStep step = tutorialSteps[currentStepIndex];

                if (CheckStepCompletion(step))
                {
                    OnStepComplete?.Invoke(currentStepIndex);
                    NextStep();
                }
            }

            // 클릭으로 타이핑 스킵
            if (isTyping && Input.GetMouseButtonDown(0))
            {
                FinishTyping();
            }
        }

        /// <summary>
        /// 튜토리얼 시작
        /// </summary>
        public void StartTutorial()
        {
            if (tutorialSteps == null || tutorialSteps.Length == 0)
            {
                Debug.LogWarning("[Tutorial] 튜토리얼 단계가 없습니다.");
                return;
            }

            isTutorialActive = true;
            currentStepIndex = -1;

            if (tutorialPanel != null)
                tutorialPanel.SetActive(true);

            OnTutorialStart?.Invoke();
            NextStep();
        }

        /// <summary>
        /// 다음 단계로 진행
        /// </summary>
        public void NextStep()
        {
            currentStepIndex++;

            if (currentStepIndex >= tutorialSteps.Length)
            {
                EndTutorial();
                return;
            }

            ShowStep(tutorialSteps[currentStepIndex]);
        }

        private void ShowStep(TutorialStep step)
        {
            // 이전 타이핑 중지
            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
            }

            // 제목
            if (titleText != null)
                titleText.text = step.title;

            // 설명 (타이핑 효과)
            if (descriptionText != null)
            {
                typingCoroutine = StartCoroutine(TypeText(step.description));
            }

            // 키 안내
            if (keyPromptText != null)
            {
                keyPromptText.text = step.keyPrompt;
                keyPromptText.gameObject.SetActive(!string.IsNullOrEmpty(step.keyPrompt));
            }

            // 하이라이트
            if (highlightImage != null)
            {
                highlightImage.gameObject.SetActive(step.highlightTarget != null);
                if (step.highlightTarget != null)
                {
                    // 하이라이트 위치 설정
                    highlightImage.rectTransform.position = step.highlightTarget.position;
                }
            }

            // 시간 제한 없으면 다음 버튼 표시
            if (nextButton != null)
            {
                nextButton.gameObject.SetActive(step.completionType == CompletionType.Button);
            }

            // 자동 진행
            if (autoAdvance && step.completionType == CompletionType.Time)
            {
                StartCoroutine(AutoAdvanceCoroutine(step.autoAdvanceTime > 0 ? step.autoAdvanceTime : autoAdvanceDelay));
            }

            Debug.Log($"[Tutorial] 단계 {currentStepIndex + 1}: {step.title}");
        }

        private IEnumerator TypeText(string text)
        {
            isTyping = true;
            descriptionText.text = "";

            foreach (char c in text)
            {
                descriptionText.text += c;
                yield return new WaitForSecondsRealtime(typingSpeed);
            }

            isTyping = false;
        }

        private void FinishTyping()
        {
            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
            }

            if (currentStepIndex >= 0 && currentStepIndex < tutorialSteps.Length)
            {
                descriptionText.text = tutorialSteps[currentStepIndex].description;
            }

            isTyping = false;
        }

        private IEnumerator AutoAdvanceCoroutine(float delay)
        {
            yield return new WaitForSecondsRealtime(delay);
            if (isTutorialActive)
            {
                NextStep();
            }
        }

        private bool CheckStepCompletion(TutorialStep step)
        {
            switch (step.completionType)
            {
                case CompletionType.KeyPress:
                    return Input.GetKeyDown(step.requiredKey);

                case CompletionType.Action:
                    // 특정 액션 완료 확인 (외부에서 호출)
                    return false;

                case CompletionType.Position:
                    if (step.targetPosition != null)
                    {
                        GameObject player = GameObject.FindGameObjectWithTag("Player");
                        if (player != null)
                        {
                            float dist = Vector3.Distance(player.transform.position, step.targetPosition.position);
                            return dist <= step.positionThreshold;
                        }
                    }
                    return false;

                default:
                    return false;
            }
        }

        /// <summary>
        /// 특정 액션 완료 알림
        /// </summary>
        public void NotifyActionComplete(string actionId)
        {
            if (!isTutorialActive) return;

            if (currentStepIndex >= 0 && currentStepIndex < tutorialSteps.Length)
            {
                TutorialStep step = tutorialSteps[currentStepIndex];
                if (step.completionType == CompletionType.Action && step.actionId == actionId)
                {
                    NextStep();
                }
            }
        }

        /// <summary>
        /// 튜토리얼 스킵
        /// </summary>
        public void SkipTutorial()
        {
            EndTutorial();
        }

        private void EndTutorial()
        {
            isTutorialActive = false;

            if (tutorialPanel != null)
                tutorialPanel.SetActive(false);

            // 완료 저장
            PlayerPrefs.SetInt("TutorialComplete", 1);
            PlayerPrefs.Save();

            OnTutorialEnd?.Invoke();
            Debug.Log("[Tutorial] 튜토리얼 완료");
        }

        /// <summary>
        /// 튜토리얼 완료 여부
        /// </summary>
        public bool HasCompletedTutorial()
        {
            return PlayerPrefs.GetInt("TutorialComplete", 0) == 1;
        }

        /// <summary>
        /// 튜토리얼 리셋
        /// </summary>
        public void ResetTutorial()
        {
            PlayerPrefs.DeleteKey("TutorialComplete");
            currentStepIndex = -1;
        }
    }

    [System.Serializable]
    public class TutorialStep
    {
        public string title = "튜토리얼";
        [TextArea(2, 5)]
        public string description = "설명";
        public string keyPrompt = "[WASD] 이동";

        [Header("완료 조건")]
        public CompletionType completionType = CompletionType.Button;
        public KeyCode requiredKey = KeyCode.None;
        public string actionId;
        public Transform targetPosition;
        public float positionThreshold = 2f;
        public float autoAdvanceTime = 0f;

        [Header("하이라이트")]
        public Transform highlightTarget;
    }

    public enum CompletionType
    {
        Button,     // 다음 버튼 클릭
        KeyPress,   // 특정 키 입력
        Action,     // 특정 행동 완료
        Position,   // 특정 위치 도달
        Time        // 시간 경과
    }
}
