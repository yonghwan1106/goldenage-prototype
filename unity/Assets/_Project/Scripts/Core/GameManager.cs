using UnityEngine;
using System;

namespace GoldenAge.Core
{
    /// <summary>
    /// 게임 상태 열거형
    /// </summary>
    public enum GameState
    {
        MainMenu,       // 메인 메뉴
        Exploration,    // 자유 탐색
        Dialogue,       // 대화 중
        Combat,         // 전투 중
        Paused,         // 일시정지
        Cutscene        // 컷씬 재생
    }

    /// <summary>
    /// 게임 전체 상태를 관리하는 매니저
    /// </summary>
    public class GameManager : Singleton<GameManager>
    {
        [Header("게임 설정")]
        [SerializeField] private GameState initialState = GameState.Exploration;

        private GameState _currentState;
        private GameState _previousState;

        public GameState CurrentState => _currentState;
        public GameState PreviousState => _previousState;

        // 이벤트
        public event Action<GameState> OnStateChanged;
        public event Action OnGamePaused;
        public event Action OnGameResumed;

        protected override void Awake()
        {
            base.Awake();
            _currentState = initialState;
        }

        private void Start()
        {
            // 초기 상태 이벤트 발생
            OnStateChanged?.Invoke(_currentState);
        }

        /// <summary>
        /// 게임 상태 변경
        /// </summary>
        public void ChangeState(GameState newState)
        {
            if (_currentState == newState) return;

            _previousState = _currentState;
            _currentState = newState;

            Debug.Log($"[GameManager] State changed: {_previousState} -> {_currentState}");

            // 상태에 따른 처리
            HandleStateChange();

            OnStateChanged?.Invoke(_currentState);
        }

        private void HandleStateChange()
        {
            switch (_currentState)
            {
                case GameState.Paused:
                    Time.timeScale = 0f;
                    OnGamePaused?.Invoke();
                    break;

                case GameState.Dialogue:
                case GameState.Cutscene:
                    // 대화/컷씬 중에는 시간 흐름 유지
                    Time.timeScale = 1f;
                    break;

                default:
                    Time.timeScale = 1f;
                    if (_previousState == GameState.Paused)
                    {
                        OnGameResumed?.Invoke();
                    }
                    break;
            }
        }

        /// <summary>
        /// 게임 일시정지
        /// </summary>
        public void PauseGame()
        {
            if (_currentState != GameState.Paused)
            {
                ChangeState(GameState.Paused);
            }
        }

        /// <summary>
        /// 게임 재개
        /// </summary>
        public void ResumeGame()
        {
            if (_currentState == GameState.Paused)
            {
                ChangeState(_previousState);
            }
        }

        /// <summary>
        /// 일시정지 토글
        /// </summary>
        public void TogglePause()
        {
            if (_currentState == GameState.Paused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }

        /// <summary>
        /// 이전 상태로 복원
        /// </summary>
        public void RestorePreviousState()
        {
            ChangeState(_previousState);
        }

        /// <summary>
        /// 특정 상태인지 확인
        /// </summary>
        public bool IsState(GameState state)
        {
            return _currentState == state;
        }

        /// <summary>
        /// 플레이어 입력이 허용되는 상태인지 확인
        /// </summary>
        public bool CanPlayerInput()
        {
            return _currentState == GameState.Exploration || _currentState == GameState.Combat;
        }
    }
}
