// 이 파일은 Unity의 Input System에서 자동 생성되어야 합니다.
// 아래는 임시 구현으로, Unity Editor에서 Input Actions 에셋을 생성한 후
// "Generate C# Class" 옵션으로 교체하세요.
//
// Input Actions 설정 방법:
// 1. Assets/Settings 폴더에서 우클릭 > Create > Input Actions
// 2. 이름을 "PlayerInputActions"로 지정
// 3. 더블클릭하여 Input Actions 에디터 열기
// 4. Player Action Map 생성 후 아래 Actions 추가:
//    - Move (Value, Vector2): WASD, Arrow Keys
//    - Look (Value, Vector2): Mouse Delta
//    - Sprint (Button): Left Shift
//    - Attack (Button): Left Mouse Button
//    - Interact (Button): E
//    - Skill1 (Button): Q
//    - Skill2 (Button): R
//    - Pause (Button): Escape
// 5. Inspector에서 "Generate C# Class" 체크하고 Apply

using UnityEngine;
using UnityEngine.InputSystem;

namespace GoldenAge.Core
{
    /// <summary>
    /// 임시 Input Actions 래퍼 클래스
    /// Unity의 New Input System 에셋으로 교체 권장
    /// </summary>
    public class PlayerInputActions
    {
        public PlayerActions Player { get; private set; }

        public PlayerInputActions()
        {
            Player = new PlayerActions();
        }

        public class PlayerActions
        {
            // Input Actions
            public InputAction Move { get; private set; }
            public InputAction Look { get; private set; }
            public InputAction Sprint { get; private set; }
            public InputAction Attack { get; private set; }
            public InputAction Interact { get; private set; }
            public InputAction Skill1 { get; private set; }
            public InputAction Skill2 { get; private set; }
            public InputAction Pause { get; private set; }

            public PlayerActions()
            {
                // Move (WASD)
                Move = new InputAction("Move", InputActionType.Value);
                Move.AddCompositeBinding("2DVector")
                    .With("Up", "<Keyboard>/w")
                    .With("Down", "<Keyboard>/s")
                    .With("Left", "<Keyboard>/a")
                    .With("Right", "<Keyboard>/d");
                Move.AddCompositeBinding("2DVector")
                    .With("Up", "<Keyboard>/upArrow")
                    .With("Down", "<Keyboard>/downArrow")
                    .With("Left", "<Keyboard>/leftArrow")
                    .With("Right", "<Keyboard>/rightArrow");

                // Look (Mouse)
                Look = new InputAction("Look", InputActionType.Value);
                Look.AddBinding("<Mouse>/delta");

                // Sprint (Left Shift)
                Sprint = new InputAction("Sprint", InputActionType.Button);
                Sprint.AddBinding("<Keyboard>/leftShift");

                // Attack (Left Mouse Button)
                Attack = new InputAction("Attack", InputActionType.Button);
                Attack.AddBinding("<Mouse>/leftButton");

                // Interact (E)
                Interact = new InputAction("Interact", InputActionType.Button);
                Interact.AddBinding("<Keyboard>/e");

                // Skill1 - Tesla Shock (Q)
                Skill1 = new InputAction("Skill1", InputActionType.Button);
                Skill1.AddBinding("<Keyboard>/q");

                // Skill2 - Ether Wave (R)
                Skill2 = new InputAction("Skill2", InputActionType.Button);
                Skill2.AddBinding("<Keyboard>/r");

                // Pause (Escape)
                Pause = new InputAction("Pause", InputActionType.Button);
                Pause.AddBinding("<Keyboard>/escape");
            }

            public void Enable()
            {
                Move.Enable();
                Look.Enable();
                Sprint.Enable();
                Attack.Enable();
                Interact.Enable();
                Skill1.Enable();
                Skill2.Enable();
                Pause.Enable();
            }

            public void Disable()
            {
                Move.Disable();
                Look.Disable();
                Sprint.Disable();
                Attack.Disable();
                Interact.Disable();
                Skill1.Disable();
                Skill2.Disable();
                Pause.Disable();
            }
        }
    }
}
