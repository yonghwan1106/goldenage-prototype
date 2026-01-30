using UnityEngine;

namespace GoldenAge.Combat
{
    /// <summary>
    /// 캐릭터(플레이어/NPC/적) 기본 데이터
    /// </summary>
    [CreateAssetMenu(fileName = "NewCharacter", menuName = "GoldenAge/Character Data")]
    public class CharacterData : ScriptableObject
    {
        [Header("기본 정보")]
        public string characterName;
        [TextArea(2, 4)]
        public string description;
        public Sprite portrait;

        [Header("스탯")]
        public int maxHealth = 100;
        public int attackPower = 10;
        public int defense = 5;
        public float moveSpeed = 4f;

        [Header("경험치 (적 전용)")]
        public int expReward = 25;

        [Header("전투")]
        public AttackData basicAttack;
        public AttackData[] skills;

        [Header("사운드")]
        public AudioClip hurtSound;
        public AudioClip deathSound;
        public AudioClip[] attackSounds;
    }
}
