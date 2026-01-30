using UnityEngine;
using GoldenAge.Core;
using GoldenAge.Player;
using GoldenAge.Combat;

namespace GoldenAge.Utilities
{
    /// <summary>
    /// 디버그/치트 명령어 (개발용)
    /// </summary>
    public class DebugCommands : MonoBehaviour
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        [Header("디버그 설정")]
        [SerializeField] private bool enableDebugCommands = true;
        [SerializeField] private bool showDebugUI = false;

        private bool godMode = false;
        private PlayerStats playerStats;

        private void Start()
        {
            playerStats = FindObjectOfType<PlayerStats>();
        }

        private void Update()
        {
            if (!enableDebugCommands) return;

            // F1: 디버그 UI 토글
            if (Input.GetKeyDown(KeyCode.F1))
            {
                showDebugUI = !showDebugUI;
                Debug.Log($"[Debug] UI: {showDebugUI}");
            }

            // F2: 무적 모드 토글
            if (Input.GetKeyDown(KeyCode.F2))
            {
                godMode = !godMode;
                Debug.Log($"[Debug] God Mode: {godMode}");
            }

            // F3: 시간 배속 순환
            if (Input.GetKeyDown(KeyCode.F3))
            {
                Time.timeScale = Time.timeScale switch
                {
                    1f => 2f,
                    2f => 0.5f,
                    _ => 1f
                };
                Debug.Log($"[Debug] Time Scale: {Time.timeScale}x");
            }

            // F4: 마우스 위치 적 즉사
            if (Input.GetKeyDown(KeyCode.F4))
            {
                KillEnemyAtMouse();
            }

            // F5: 빠른 저장
            if (Input.GetKeyDown(KeyCode.F5))
            {
                Debug.Log("[Debug] Quick Save (Not implemented)");
            }

            // F6: 체력/에너지 회복
            if (Input.GetKeyDown(KeyCode.F6))
            {
                if (playerStats != null)
                {
                    playerStats.Heal(9999);
                    playerStats.RestoreEnergy(9999);
                    Debug.Log("[Debug] Full Restore");
                }
            }

            // F7: 경험치 추가
            if (Input.GetKeyDown(KeyCode.F7))
            {
                if (playerStats != null)
                {
                    playerStats.AddExperience(100);
                    Debug.Log("[Debug] +100 EXP");
                }
            }

            // F8: 모든 적 처치
            if (Input.GetKeyDown(KeyCode.F8))
            {
                CombatManager.Instance?.KillAllEnemies();
                Debug.Log("[Debug] All enemies killed");
            }

            // F9: 빠른 불러오기
            if (Input.GetKeyDown(KeyCode.F9))
            {
                Debug.Log("[Debug] Quick Load (Not implemented)");
            }
        }

        private void KillEnemyAtMouse()
        {
            Camera cam = Camera.main;
            if (cam == null) return;

            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 100f))
            {
                if (hit.collider.TryGetComponent<EnemyAI>(out var enemy))
                {
                    enemy.TakeDamage(99999, DamageType.Physical);
                    Debug.Log($"[Debug] Killed: {enemy.name}");
                }
            }
        }

        private void OnGUI()
        {
            if (!showDebugUI) return;

            GUILayout.BeginArea(new Rect(10, 10, 300, 500));
            GUILayout.Box("=== Debug Info ===");

            // 플레이어 정보
            if (playerStats != null)
            {
                GUILayout.Label($"HP: {playerStats.CurrentHealth}/{playerStats.MaxHealth}");
                GUILayout.Label($"Energy: {playerStats.CurrentEnergy}/{playerStats.MaxEnergy}");
                GUILayout.Label($"Level: {playerStats.Level}");
                GUILayout.Label($"EXP: {playerStats.Experience}/{playerStats.ExperienceToNextLevel}");
            }

            GUILayout.Space(10);

            // 게임 상태
            GUILayout.Label($"Game State: {GameManager.Instance?.CurrentState}");
            GUILayout.Label($"In Combat: {CombatManager.Instance?.IsInCombat}");
            GUILayout.Label($"Active Enemies: {CombatManager.Instance?.ActiveEnemyCount}");

            GUILayout.Space(10);

            // 디버그 상태
            GUILayout.Label($"God Mode: {godMode}");
            GUILayout.Label($"Time Scale: {Time.timeScale}x");
            GUILayout.Label($"FPS: {(1f / Time.unscaledDeltaTime):F0}");

            GUILayout.Space(10);

            // 단축키 안내
            GUILayout.Box("=== Shortcuts ===");
            GUILayout.Label("F1: Toggle Debug UI");
            GUILayout.Label("F2: Toggle God Mode");
            GUILayout.Label("F3: Cycle Time Scale");
            GUILayout.Label("F4: Kill Enemy at Mouse");
            GUILayout.Label("F6: Full Restore");
            GUILayout.Label("F7: Add 100 EXP");
            GUILayout.Label("F8: Kill All Enemies");

            GUILayout.EndArea();
        }
#endif
    }
}
