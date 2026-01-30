using UnityEngine;

namespace GoldenAge.Combat
{
    /// <summary>
    /// 상태이상 효과 타입
    /// </summary>
    public enum EffectType
    {
        None,
        Stun,           // 행동 불가
        Slow,           // 이동속도 감소
        Electrified,    // 전기 감전 (도트)
        DimensionMark   // 차원 표식 (융합 콤보용)
    }

    /// <summary>
    /// 상태이상 효과 데이터
    /// </summary>
    [CreateAssetMenu(fileName = "NewStatusEffect", menuName = "GoldenAge/Status Effect")]
    public class StatusEffect : ScriptableObject
    {
        [Header("기본 정보")]
        public string effectName;
        public EffectType effectType;
        public Sprite icon;

        [Header("지속 시간")]
        public float duration = 3f;
        public float tickInterval = 1f;

        [Header("효과 수치")]
        public int damagePerTick = 0;
        [Range(0f, 1f)]
        public float slowPercent = 0f;
        public bool isStunned = false;

        [Header("비주얼")]
        public GameObject effectVFX;
        public AudioClip effectSFX;
    }

    /// <summary>
    /// 활성화된 상태이상 효과 (런타임 데이터)
    /// </summary>
    [System.Serializable]
    public class ActiveEffect
    {
        public StatusEffect effect;
        public float remainingDuration;
        public float nextTickTime;
        public GameObject vfxInstance;

        public ActiveEffect(StatusEffect effect)
        {
            this.effect = effect;
            this.remainingDuration = effect.duration;
            this.nextTickTime = Time.time + effect.tickInterval;
        }

        public bool IsExpired => remainingDuration <= 0;

        public void Update(float deltaTime)
        {
            remainingDuration -= deltaTime;
        }

        public bool ShouldTick()
        {
            if (Time.time >= nextTickTime)
            {
                nextTickTime = Time.time + effect.tickInterval;
                return true;
            }
            return false;
        }
    }
}
