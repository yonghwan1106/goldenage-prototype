namespace GoldenAge.Combat
{
    /// <summary>
    /// 데미지 타입 열거형
    /// </summary>
    public enum DamageType
    {
        Physical,       // 물리
        Electric,       // 전기
        Dimensional,    // 차원
        Fusion          // 융합
    }

    /// <summary>
    /// 데미지를 받을 수 있는 오브젝트가 구현해야 하는 인터페이스
    /// </summary>
    public interface IDamageable
    {
        /// <summary>
        /// 데미지 받기
        /// </summary>
        void TakeDamage(int damage, DamageType damageType);

        /// <summary>
        /// 상태이상 효과 적용
        /// </summary>
        void ApplyEffect(StatusEffect effect);

        /// <summary>
        /// 현재 체력 (UI 표시용)
        /// </summary>
        int CurrentHealth { get; }

        /// <summary>
        /// 최대 체력 (UI 표시용)
        /// </summary>
        int MaxHealth { get; }

        /// <summary>
        /// 생존 여부
        /// </summary>
        bool IsAlive { get; }
    }

    /// <summary>
    /// 공격을 수행할 수 있는 오브젝트가 구현해야 하는 인터페이스
    /// </summary>
    public interface IAttacker
    {
        /// <summary>
        /// 대상 공격
        /// </summary>
        void Attack(IDamageable target);
    }
}
