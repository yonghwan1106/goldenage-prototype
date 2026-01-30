using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

namespace GoldenAge.Core
{
    /// <summary>
    /// 간단한 컷신/시퀀스 시스템
    /// </summary>
    public class CutsceneSystem : Singleton<CutsceneSystem>
    {
        [Header("UI")]
        [SerializeField] private GameObject cutscenePanel;
        [SerializeField] private Image blackBars;
        [SerializeField] private TextMeshProUGUI subtitleText;
        [SerializeField] private CanvasGroup fadeOverlay;

        [Header("설정")]
        [SerializeField] private float blackBarHeight = 80f;
        [SerializeField] private float transitionDuration = 0.5f;
        [SerializeField] private float defaultSubtitleDuration = 3f;

        [Header("카메라")]
        [SerializeField] private Camera cutsceneCamera;

        private bool isPlaying = false;
        private Cutscene currentCutscene;
        private Coroutine cutsceneCoroutine;

        public bool IsPlaying => isPlaying;

        public event System.Action OnCutsceneStart;
        public event System.Action OnCutsceneEnd;

        protected override void Awake()
        {
            base.Awake();

            if (cutscenePanel != null)
                cutscenePanel.SetActive(false);

            if (subtitleText != null)
                subtitleText.text = "";
        }

        /// <summary>
        /// 컷신 재생
        /// </summary>
        public void PlayCutscene(Cutscene cutscene)
        {
            if (isPlaying || cutscene == null) return;

            currentCutscene = cutscene;
            cutsceneCoroutine = StartCoroutine(PlayCutsceneCoroutine());
        }

        /// <summary>
        /// 컷신 스킵
        /// </summary>
        public void SkipCutscene()
        {
            if (!isPlaying || currentCutscene == null) return;

            if (currentCutscene.canSkip)
            {
                if (cutsceneCoroutine != null)
                {
                    StopCoroutine(cutsceneCoroutine);
                }
                EndCutscene();
            }
        }

        private IEnumerator PlayCutsceneCoroutine()
        {
            isPlaying = true;

            // 게임 상태 변경
            GameManager.Instance?.SetState(GameState.Cutscene);
            OnCutsceneStart?.Invoke();

            // UI 표시
            if (cutscenePanel != null)
                cutscenePanel.SetActive(true);

            // 블랙바 애니메이션
            yield return StartCoroutine(ShowBlackBars());

            // 각 액션 실행
            foreach (var action in currentCutscene.actions)
            {
                yield return StartCoroutine(ExecuteAction(action));
            }

            // 종료
            EndCutscene();
        }

        private IEnumerator ExecuteAction(CutsceneAction action)
        {
            switch (action.type)
            {
                case CutsceneActionType.Wait:
                    yield return new WaitForSeconds(action.duration);
                    break;

                case CutsceneActionType.Fade:
                    yield return StartCoroutine(Fade(action.fadeIn, action.duration));
                    break;

                case CutsceneActionType.Subtitle:
                    yield return StartCoroutine(ShowSubtitle(action.text, action.duration));
                    break;

                case CutsceneActionType.MoveCamera:
                    yield return StartCoroutine(MoveCamera(action.targetTransform, action.duration));
                    break;

                case CutsceneActionType.PlaySound:
                    if (action.audioClip != null)
                    {
                        AudioSource.PlayClipAtPoint(action.audioClip, Camera.main.transform.position);
                    }
                    break;

                case CutsceneActionType.PlayAnimation:
                    if (action.targetAnimator != null && !string.IsNullOrEmpty(action.animationTrigger))
                    {
                        action.targetAnimator.SetTrigger(action.animationTrigger);
                    }
                    if (action.waitForAnimation)
                    {
                        yield return new WaitForSeconds(action.duration);
                    }
                    break;

                case CutsceneActionType.SetActive:
                    if (action.targetObject != null)
                    {
                        action.targetObject.SetActive(action.setActive);
                    }
                    break;

                case CutsceneActionType.TeleportPlayer:
                    TeleportPlayer(action.targetTransform);
                    break;

                case CutsceneActionType.StartDialogue:
                    if (action.dialogueData != null)
                    {
                        Dialogue.DialogueManager.Instance?.StartDialogue(action.dialogueData);
                        // 대화 종료 대기
                        yield return new WaitUntil(() =>
                            Dialogue.DialogueManager.Instance == null ||
                            !Dialogue.DialogueManager.Instance.IsDialogueActive);
                    }
                    break;

                case CutsceneActionType.CustomEvent:
                    action.customEvent?.Invoke();
                    break;
            }
        }

        private IEnumerator ShowBlackBars()
        {
            if (blackBars == null) yield break;

            RectTransform rt = blackBars.rectTransform;
            float elapsed = 0f;

            while (elapsed < transitionDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / transitionDuration;
                // 블랙바 높이 애니메이션
                yield return null;
            }
        }

        private IEnumerator HideBlackBars()
        {
            if (blackBars == null) yield break;

            float elapsed = 0f;

            while (elapsed < transitionDuration)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }
        }

        private IEnumerator Fade(bool fadeIn, float duration)
        {
            if (fadeOverlay == null) yield break;

            float startAlpha = fadeIn ? 1f : 0f;
            float endAlpha = fadeIn ? 0f : 1f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                fadeOverlay.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / duration);
                yield return null;
            }

            fadeOverlay.alpha = endAlpha;
        }

        private IEnumerator ShowSubtitle(string text, float duration)
        {
            if (subtitleText == null) yield break;

            subtitleText.text = text;

            // 페이드 인
            Color color = subtitleText.color;
            color.a = 0f;
            subtitleText.color = color;

            float fadeTime = 0.3f;
            float elapsed = 0f;

            while (elapsed < fadeTime)
            {
                elapsed += Time.deltaTime;
                color.a = elapsed / fadeTime;
                subtitleText.color = color;
                yield return null;
            }

            color.a = 1f;
            subtitleText.color = color;

            // 대기
            yield return new WaitForSeconds(duration - fadeTime * 2);

            // 페이드 아웃
            elapsed = 0f;
            while (elapsed < fadeTime)
            {
                elapsed += Time.deltaTime;
                color.a = 1f - (elapsed / fadeTime);
                subtitleText.color = color;
                yield return null;
            }

            subtitleText.text = "";
        }

        private IEnumerator MoveCamera(Transform target, float duration)
        {
            if (target == null) yield break;

            Camera cam = cutsceneCamera != null ? cutsceneCamera : Camera.main;
            if (cam == null) yield break;

            Vector3 startPos = cam.transform.position;
            Quaternion startRot = cam.transform.rotation;

            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);

                cam.transform.position = Vector3.Lerp(startPos, target.position, t);
                cam.transform.rotation = Quaternion.Slerp(startRot, target.rotation, t);

                yield return null;
            }

            cam.transform.position = target.position;
            cam.transform.rotation = target.rotation;
        }

        private void TeleportPlayer(Transform target)
        {
            if (target == null) return;

            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                CharacterController cc = player.GetComponent<CharacterController>();
                if (cc != null) cc.enabled = false;

                player.transform.position = target.position;
                player.transform.rotation = target.rotation;

                if (cc != null) cc.enabled = true;
            }
        }

        private void EndCutscene()
        {
            isPlaying = false;

            // 블랙바 숨기기
            StartCoroutine(HideBlackBars());

            // UI 숨기기
            if (cutscenePanel != null)
                cutscenePanel.SetActive(false);

            if (subtitleText != null)
                subtitleText.text = "";

            // 게임 상태 복원
            GameManager.Instance?.SetState(GameState.Exploration);

            OnCutsceneEnd?.Invoke();
            currentCutscene = null;
        }
    }

    [System.Serializable]
    public class Cutscene
    {
        public string cutsceneName;
        public bool canSkip = true;
        public List<CutsceneAction> actions = new List<CutsceneAction>();
    }

    [System.Serializable]
    public class CutsceneAction
    {
        public CutsceneActionType type;
        public float duration = 1f;

        [Header("Fade")]
        public bool fadeIn = true;

        [Header("Subtitle")]
        [TextArea] public string text;

        [Header("Camera/Teleport")]
        public Transform targetTransform;

        [Header("Sound")]
        public AudioClip audioClip;

        [Header("Animation")]
        public Animator targetAnimator;
        public string animationTrigger;
        public bool waitForAnimation;

        [Header("SetActive")]
        public GameObject targetObject;
        public bool setActive;

        [Header("Dialogue")]
        public Dialogue.DialogueData dialogueData;

        [Header("Custom")]
        public UnityEngine.Events.UnityEvent customEvent;
    }

    public enum CutsceneActionType
    {
        Wait,
        Fade,
        Subtitle,
        MoveCamera,
        PlaySound,
        PlayAnimation,
        SetActive,
        TeleportPlayer,
        StartDialogue,
        CustomEvent
    }
}
