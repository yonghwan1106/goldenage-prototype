using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GoldenAge.Utilities
{
    /// <summary>
    /// 런타임 디버그 콘솔
    /// </summary>
    public class DebugConsole : MonoBehaviour
    {
        public static DebugConsole Instance { get; private set; }

        [Header("UI 참조")]
        [SerializeField] private GameObject consolePanel;
        [SerializeField] private TMP_InputField inputField;
        [SerializeField] private TextMeshProUGUI outputText;
        [SerializeField] private ScrollRect scrollRect;

        [Header("설정")]
        [SerializeField] private KeyCode toggleKey = KeyCode.BackQuote; // ` 키
        [SerializeField] private int maxLogLines = 100;
        [SerializeField] private bool showUnityLogs = true;

        [Header("색상")]
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color warningColor = Color.yellow;
        [SerializeField] private Color errorColor = Color.red;
        [SerializeField] private Color successColor = Color.green;
        [SerializeField] private Color commandColor = Color.cyan;

        private bool isOpen = false;
        private List<string> logHistory = new List<string>();
        private List<string> commandHistory = new List<string>();
        private int historyIndex = -1;

        private Dictionary<string, Action<string[]>> commands = new Dictionary<string, Action<string[]>>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            RegisterDefaultCommands();

            if (consolePanel != null)
                consolePanel.SetActive(false);

            if (showUnityLogs)
                Application.logMessageReceived += HandleUnityLog;
        }

        private void OnDestroy()
        {
            if (showUnityLogs)
                Application.logMessageReceived -= HandleUnityLog;
        }

        private void Update()
        {
            // 콘솔 토글
            if (Input.GetKeyDown(toggleKey))
            {
                ToggleConsole();
            }

            if (!isOpen) return;

            // 명령어 히스토리 탐색
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                NavigateHistory(-1);
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                NavigateHistory(1);
            }

            // 엔터로 명령 실행
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                ExecuteInput();
            }
        }

        public void ToggleConsole()
        {
            isOpen = !isOpen;

            if (consolePanel != null)
                consolePanel.SetActive(isOpen);

            if (isOpen)
            {
                Time.timeScale = 0f;
                if (inputField != null)
                {
                    inputField.ActivateInputField();
                    inputField.Select();
                }
            }
            else
            {
                Time.timeScale = 1f;
            }
        }

        private void ExecuteInput()
        {
            if (inputField == null) return;

            string input = inputField.text.Trim();
            if (string.IsNullOrEmpty(input)) return;

            // 히스토리에 추가
            commandHistory.Add(input);
            historyIndex = commandHistory.Count;

            // 명령어 표시
            Log($"> {input}", commandColor);

            // 파싱 및 실행
            string[] parts = input.Split(' ');
            string cmd = parts[0].ToLower();
            string[] args = parts.Skip(1).ToArray();

            if (commands.TryGetValue(cmd, out Action<string[]> action))
            {
                try
                {
                    action(args);
                }
                catch (Exception e)
                {
                    LogError($"명령 실행 오류: {e.Message}");
                }
            }
            else
            {
                LogWarning($"알 수 없는 명령: {cmd}. 'help' 입력하여 명령 목록 확인");
            }

            // 입력 필드 초기화
            inputField.text = "";
            inputField.ActivateInputField();
        }

        private void NavigateHistory(int direction)
        {
            if (commandHistory.Count == 0) return;

            historyIndex = Mathf.Clamp(historyIndex + direction, 0, commandHistory.Count - 1);

            if (inputField != null)
            {
                inputField.text = commandHistory[historyIndex];
                inputField.caretPosition = inputField.text.Length;
            }
        }

        #region Logging

        public void Log(string message, Color? color = null)
        {
            Color c = color ?? normalColor;
            string colorHex = ColorUtility.ToHtmlStringRGB(c);
            string formattedLog = $"<color=#{colorHex}>{message}</color>";

            logHistory.Add(formattedLog);

            // 최대 라인 수 제한
            while (logHistory.Count > maxLogLines)
            {
                logHistory.RemoveAt(0);
            }

            UpdateOutput();
        }

        public void LogWarning(string message) => Log($"[경고] {message}", warningColor);
        public void LogError(string message) => Log($"[오류] {message}", errorColor);
        public void LogSuccess(string message) => Log($"[성공] {message}", successColor);

        private void HandleUnityLog(string logString, string stackTrace, LogType type)
        {
            Color color = type switch
            {
                LogType.Warning => warningColor,
                LogType.Error => errorColor,
                LogType.Exception => errorColor,
                _ => normalColor
            };

            Log($"[Unity] {logString}", color);
        }

        private void UpdateOutput()
        {
            if (outputText == null) return;

            outputText.text = string.Join("\n", logHistory);

            // 스크롤 맨 아래로
            if (scrollRect != null)
            {
                Canvas.ForceUpdateCanvases();
                scrollRect.verticalNormalizedPosition = 0f;
            }
        }

        public void ClearLog()
        {
            logHistory.Clear();
            UpdateOutput();
        }

        #endregion

        #region Commands

        /// <summary>
        /// 명령어 등록
        /// </summary>
        public void RegisterCommand(string name, Action<string[]> action, string description = "")
        {
            commands[name.ToLower()] = action;
        }

        private void RegisterDefaultCommands()
        {
            RegisterCommand("help", CmdHelp, "명령어 목록 표시");
            RegisterCommand("clear", CmdClear, "콘솔 지우기");
            RegisterCommand("god", CmdGodMode, "무적 모드 토글");
            RegisterCommand("heal", CmdHeal, "체력/에너지 회복");
            RegisterCommand("kill", CmdKillAll, "모든 적 처치");
            RegisterCommand("spawn", CmdSpawn, "적 스폰: spawn [수량]");
            RegisterCommand("level", CmdLevel, "레벨 설정: level [숫자]");
            RegisterCommand("exp", CmdExp, "경험치 추가: exp [양]");
            RegisterCommand("tp", CmdTeleport, "텔레포트: tp [x] [y] [z]");
            RegisterCommand("time", CmdTimeScale, "시간 배속: time [배율]");
            RegisterCommand("fps", CmdFPS, "FPS 표시 토글");
            RegisterCommand("quest", CmdQuest, "퀘스트 관리: quest [complete/list]");
            RegisterCommand("item", CmdItem, "아이템 추가: item [이름] [수량]");
            RegisterCommand("save", CmdSave, "게임 저장");
            RegisterCommand("load", CmdLoad, "게임 불러오기");
        }

        private void CmdHelp(string[] args)
        {
            Log("=== 사용 가능한 명령어 ===", successColor);
            Log("help - 명령어 목록");
            Log("clear - 콘솔 지우기");
            Log("god - 무적 모드 토글");
            Log("heal - 체력/에너지 회복");
            Log("kill - 모든 적 처치");
            Log("spawn [수량] - 적 스폰");
            Log("level [숫자] - 레벨 설정");
            Log("exp [양] - 경험치 추가");
            Log("tp [x] [y] [z] - 텔레포트");
            Log("time [배율] - 시간 배속");
            Log("fps - FPS 표시 토글");
            Log("quest [complete/list] - 퀘스트 관리");
            Log("item [이름] [수량] - 아이템 추가");
            Log("save - 게임 저장");
            Log("load - 게임 불러오기");
        }

        private void CmdClear(string[] args) => ClearLog();

        private void CmdGodMode(string[] args)
        {
            // PlayerStats에 godMode 필드가 있다고 가정
            LogSuccess("무적 모드 토글 (구현 필요)");
        }

        private void CmdHeal(string[] args)
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                var stats = player.GetComponent<Player.PlayerStats>();
                if (stats != null)
                {
                    stats.Heal(9999);
                    stats.RestoreEnergy(9999);
                    LogSuccess("체력/에너지 회복됨");
                    return;
                }
            }
            LogError("플레이어를 찾을 수 없음");
        }

        private void CmdKillAll(string[] args)
        {
            Combat.CombatManager.Instance?.KillAllEnemies();
            LogSuccess("모든 적 처치됨");
        }

        private void CmdSpawn(string[] args)
        {
            int count = 1;
            if (args.Length > 0) int.TryParse(args[0], out count);
            LogSuccess($"적 {count}명 스폰 (구현 필요)");
        }

        private void CmdLevel(string[] args)
        {
            if (args.Length == 0)
            {
                LogWarning("사용법: level [숫자]");
                return;
            }

            if (int.TryParse(args[0], out int level))
            {
                var player = GameObject.FindGameObjectWithTag("Player");
                var stats = player?.GetComponent<Player.PlayerStats>();
                if (stats != null)
                {
                    stats.SetLevel(level);
                    LogSuccess($"레벨 {level}로 설정됨");
                    return;
                }
            }
            LogError("레벨 설정 실패");
        }

        private void CmdExp(string[] args)
        {
            if (args.Length == 0)
            {
                LogWarning("사용법: exp [양]");
                return;
            }

            if (int.TryParse(args[0], out int exp))
            {
                var player = GameObject.FindGameObjectWithTag("Player");
                var stats = player?.GetComponent<Player.PlayerStats>();
                if (stats != null)
                {
                    stats.AddExperience(exp);
                    LogSuccess($"경험치 +{exp}");
                    return;
                }
            }
            LogError("경험치 추가 실패");
        }

        private void CmdTeleport(string[] args)
        {
            if (args.Length < 3)
            {
                LogWarning("사용법: tp [x] [y] [z]");
                return;
            }

            if (float.TryParse(args[0], out float x) &&
                float.TryParse(args[1], out float y) &&
                float.TryParse(args[2], out float z))
            {
                var player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    player.transform.position = new Vector3(x, y, z);
                    LogSuccess($"텔레포트: ({x}, {y}, {z})");
                    return;
                }
            }
            LogError("텔레포트 실패");
        }

        private void CmdTimeScale(string[] args)
        {
            if (args.Length == 0)
            {
                Log($"현재 시간 배율: {Time.timeScale}x");
                return;
            }

            if (float.TryParse(args[0], out float scale))
            {
                Time.timeScale = Mathf.Clamp(scale, 0f, 10f);
                LogSuccess($"시간 배율: {Time.timeScale}x");
            }
        }

        private void CmdFPS(string[] args)
        {
            var monitor = FindObjectOfType<PerformanceMonitor>();
            if (monitor != null)
            {
                monitor.ToggleDisplay();
                LogSuccess("FPS 표시 토글됨");
            }
            else
            {
                LogWarning("PerformanceMonitor를 찾을 수 없음");
            }
        }

        private void CmdQuest(string[] args)
        {
            if (args.Length == 0)
            {
                LogWarning("사용법: quest [complete/list]");
                return;
            }

            var qm = Quest.QuestManager.Instance;
            if (qm == null)
            {
                LogError("QuestManager를 찾을 수 없음");
                return;
            }

            switch (args[0].ToLower())
            {
                case "list":
                    var active = qm.GetActiveQuestIds();
                    Log($"활성 퀘스트: {string.Join(", ", active)}");
                    break;
                case "complete":
                    LogSuccess("퀘스트 완료 (구현 필요)");
                    break;
            }
        }

        private void CmdItem(string[] args)
        {
            LogSuccess("아이템 추가 (구현 필요)");
        }

        private void CmdSave(string[] args)
        {
            Core.SaveSystem.Instance?.SaveGame();
            LogSuccess("게임 저장됨");
        }

        private void CmdLoad(string[] args)
        {
            Core.SaveSystem.Instance?.LoadGame();
            LogSuccess("게임 불러옴");
        }

        #endregion
    }
}
