using UnityEngine;

namespace GoldenAge.Player
{
    /// <summary>
    /// 플레이어 애니메이션을 제어하는 클래스
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class PlayerAnimator : MonoBehaviour
    {
        private Animator animator;
        private PlayerMovement movement;

        // 애니메이터 파라미터 해시
        private static readonly int SpeedHash = Animator.StringToHash("Speed");
        private static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");
        private static readonly int IsRunningHash = Animator.StringToHash("IsRunning");
        private static readonly int AttackTriggerHash = Animator.StringToHash("Attack");
        private static readonly int Skill1TriggerHash = Animator.StringToHash("Skill1");
        private static readonly int Skill2TriggerHash = Animator.StringToHash("Skill2");
        private static readonly int HitTriggerHash = Animator.StringToHash("Hit");
        private static readonly int DeathTriggerHash = Animator.StringToHash("Death");
        private static readonly int InteractTriggerHash = Animator.StringToHash("Interact");

        [Header("애니메이션 블렌드 설정")]
        [SerializeField] private float speedDampTime = 0.1f;

        private void Awake()
        {
            animator = GetComponent<Animator>();
            movement = GetComponent<PlayerMovement>();
        }

        private void Update()
        {
            if (movement == null || animator == null) return;

            UpdateLocomotion();
        }

        private void UpdateLocomotion()
        {
            // 속도에 따른 블렌드
            float normalizedSpeed = movement.IsRunning() ? 1f :
                                   movement.IsMoving() ? 0.5f : 0f;

            animator.SetFloat(SpeedHash, normalizedSpeed, speedDampTime, Time.deltaTime);
            animator.SetBool(IsGroundedHash, movement.IsGrounded);
            animator.SetBool(IsRunningHash, movement.IsRunning());
        }

        #region Animation Triggers

        /// <summary>
        /// 공격 애니메이션 트리거
        /// </summary>
        public void TriggerAttack()
        {
            animator.SetTrigger(AttackTriggerHash);
        }

        /// <summary>
        /// 스킬 1 (테슬라 충격기) 애니메이션 트리거
        /// </summary>
        public void TriggerSkill1()
        {
            animator.SetTrigger(Skill1TriggerHash);
        }

        /// <summary>
        /// 스킬 2 (에테르 파동) 애니메이션 트리거
        /// </summary>
        public void TriggerSkill2()
        {
            animator.SetTrigger(Skill2TriggerHash);
        }

        /// <summary>
        /// 피격 애니메이션 트리거
        /// </summary>
        public void TriggerHit()
        {
            animator.SetTrigger(HitTriggerHash);
        }

        /// <summary>
        /// 사망 애니메이션 트리거
        /// </summary>
        public void TriggerDeath()
        {
            animator.SetTrigger(DeathTriggerHash);
        }

        /// <summary>
        /// 상호작용 애니메이션 트리거
        /// </summary>
        public void TriggerInteract()
        {
            animator.SetTrigger(InteractTriggerHash);
        }

        #endregion

        /// <summary>
        /// 애니메이터 속도 설정 (슬로우 모션 등)
        /// </summary>
        public void SetAnimatorSpeed(float speed)
        {
            animator.speed = speed;
        }

        /// <summary>
        /// 특정 애니메이션 상태인지 확인
        /// </summary>
        public bool IsInState(string stateName, int layer = 0)
        {
            return animator.GetCurrentAnimatorStateInfo(layer).IsName(stateName);
        }
    }
}
