using UnityEngine;
using UnityEngine.InputSystem;
using System;
using System.Collections.Generic;

namespace GoldenAge.Core
{
    /// <summary>
    /// 입력 리바인딩 시스템
    /// </summary>
    public class InputRebindSystem : Singleton<InputRebindSystem>
    {
        [Header("입력 에셋")]
        [SerializeField] private InputActionAsset inputActions;

        private InputActionRebindingExtensions.RebindingOperation rebindOperation;
        private const string RebindsKey = "InputRebinds";

        public event Action<InputAction, int> OnRebindStarted;
        public event Action<InputAction, int> OnRebindCompleted;
        public event Action OnRebindCanceled;

        protected override void Awake()
        {
            base.Awake();
            LoadRebinds();
        }

        private void OnDestroy()
        {
            CleanupOperation();
        }

        /// <summary>
        /// 리바인딩 시작
        /// </summary>
        public void StartRebind(InputAction action, int bindingIndex)
        {
            if (action == null) return;

            // 기존 작업 정리
            CleanupOperation();

            // 액션 비활성화
            action.Disable();

            OnRebindStarted?.Invoke(action, bindingIndex);

            rebindOperation = action.PerformInteractiveRebinding(bindingIndex)
                .WithControlsExcluding("Mouse")
                .WithCancelingThrough("<Keyboard>/escape")
                .OnMatchWaitForAnother(0.1f)
                .OnComplete(operation => OnRebindComplete(action, bindingIndex))
                .OnCancel(operation => OnRebindCancel(action))
                .Start();
        }

        private void OnRebindComplete(InputAction action, int bindingIndex)
        {
            CleanupOperation();
            action.Enable();

            SaveRebinds();
            OnRebindCompleted?.Invoke(action, bindingIndex);

            Debug.Log($"[InputRebind] 리바인딩 완료: {action.name} -> {action.bindings[bindingIndex].effectivePath}");
        }

        private void OnRebindCancel(InputAction action)
        {
            CleanupOperation();
            action.Enable();

            OnRebindCanceled?.Invoke();
            Debug.Log("[InputRebind] 리바인딩 취소됨");
        }

        private void CleanupOperation()
        {
            rebindOperation?.Dispose();
            rebindOperation = null;
        }

        /// <summary>
        /// 기본 바인딩으로 초기화
        /// </summary>
        public void ResetToDefaults()
        {
            if (inputActions == null) return;

            foreach (var map in inputActions.actionMaps)
            {
                map.RemoveAllBindingOverrides();
            }

            PlayerPrefs.DeleteKey(RebindsKey);
            PlayerPrefs.Save();

            Debug.Log("[InputRebind] 기본 설정으로 초기화됨");
        }

        /// <summary>
        /// 특정 액션 초기화
        /// </summary>
        public void ResetAction(InputAction action)
        {
            if (action == null) return;

            action.RemoveAllBindingOverrides();
            SaveRebinds();
        }

        /// <summary>
        /// 바인딩 저장
        /// </summary>
        public void SaveRebinds()
        {
            if (inputActions == null) return;

            string rebinds = inputActions.SaveBindingOverridesAsJson();
            PlayerPrefs.SetString(RebindsKey, rebinds);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// 바인딩 로드
        /// </summary>
        public void LoadRebinds()
        {
            if (inputActions == null) return;

            string rebinds = PlayerPrefs.GetString(RebindsKey, "");
            if (!string.IsNullOrEmpty(rebinds))
            {
                inputActions.LoadBindingOverridesFromJson(rebinds);
                Debug.Log("[InputRebind] 저장된 바인딩 로드됨");
            }
        }

        /// <summary>
        /// 바인딩 표시 이름 가져오기
        /// </summary>
        public string GetBindingDisplayName(InputAction action, int bindingIndex)
        {
            if (action == null || bindingIndex < 0 || bindingIndex >= action.bindings.Count)
                return "";

            return InputControlPath.ToHumanReadableString(
                action.bindings[bindingIndex].effectivePath,
                InputControlPath.HumanReadableStringOptions.OmitDevice);
        }

        /// <summary>
        /// 중복 바인딩 확인
        /// </summary>
        public InputAction CheckDuplicateBinding(InputAction action, int bindingIndex)
        {
            if (inputActions == null || action == null) return null;

            InputBinding newBinding = action.bindings[bindingIndex];

            foreach (var map in inputActions.actionMaps)
            {
                foreach (var otherAction in map.actions)
                {
                    if (otherAction == action) continue;

                    for (int i = 0; i < otherAction.bindings.Count; i++)
                    {
                        if (otherAction.bindings[i].effectivePath == newBinding.effectivePath)
                        {
                            return otherAction;
                        }
                    }
                }
            }

            return null;
        }
    }
}
