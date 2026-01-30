using UnityEngine;

namespace GoldenAge.Combat
{
    /// <summary>
    /// 공격 타입 열거형
    /// </summary>
    public enum AttackType
    {
        Melee,      // 근접
        Ranged,     // 원거리
        Science,    // 과학
        Magic,      // 마법
        Fusion      // 융합
    }

    /// <summary>
    /// AOE 형태 열거형
    /// </summary>
    public enum AOEShape
    {
        Circle,     // 원형
        Cone,       // 원뿔형
        Line        // 직선
    }

    /// <summary>
    /// 공격 데이터 ScriptableObject
    /// </summary>
    [CreateAssetMenu(fileName = "NewAttack", menuName = "GoldenAge/Attack Data")]
    public class AttackData : ScriptableObject
    {
        [Header("기본 정보")]
        public string attackName;
        public string description;
        public Sprite icon;
        public AttackType attackType;
        public DamageType damageType;

        [Header("스탯")]
        public int baseDamage = 10;
        public float range = 2f;
        public float cooldown = 1f;
        public float castTime = 0f;
        public float animationDuration = 0.5f;

        [Header("에너지 소모")]
        public int energyCost = 0;

        [Header("범위 공격")]
        public bool isAOE = false;
        public float aoeRadius = 0f;
        public AOEShape aoeShape = AOEShape.Circle;
        public float coneAngle = 45f; // Cone 형태일 때 각도

        [Header("효과")]
        public StatusEffect[] applyEffects;
        public float effectChance = 1f; // 효과 적용 확률

        [Header("비주얼/사운드")]
        public GameObject castVFX;
        public GameObject hitVFX;
        public AudioClip castSFX;
        public AudioClip hitSFX;

        [Header("애니메이션")]
        public string animationTrigger;

        /// <summary>
        /// 쿨다운 중인지 확인하기 위한 헬퍼
        /// </summary>
        public bool IsReady(float lastUseTime)
        {
            return Time.time >= lastUseTime + cooldown;
        }

        /// <summary>
        /// 남은 쿨다운 시간
        /// </summary>
        public float GetRemainingCooldown(float lastUseTime)
        {
            float remaining = (lastUseTime + cooldown) - Time.time;
            return Mathf.Max(0f, remaining);
        }

        /// <summary>
        /// 쿨다운 진행률 (0~1, UI용)
        /// </summary>
        public float GetCooldownProgress(float lastUseTime)
        {
            if (cooldown <= 0) return 0f;
            float remaining = GetRemainingCooldown(lastUseTime);
            return remaining / cooldown;
        }
    }
}
