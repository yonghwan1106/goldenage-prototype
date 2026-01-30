using UnityEngine;

namespace GoldenAge.Combat
{
    /// <summary>
    /// 아이템 타입
    /// </summary>
    public enum ItemType
    {
        Weapon,
        Consumable,
        Quest,
        Key
    }

    /// <summary>
    /// 아이템 데이터 ScriptableObject
    /// </summary>
    [CreateAssetMenu(fileName = "NewItem", menuName = "GoldenAge/Item Data")]
    public class ItemData : ScriptableObject
    {
        [Header("기본 정보")]
        public string itemID;
        public string itemName;

        [TextArea(2, 4)]
        public string description;

        public Sprite icon;
        public ItemType itemType;

        [Header("스택")]
        public int maxStack = 1;
        public bool isConsumable;

        [Header("효과 (소모품인 경우)")]
        public int healAmount;
        public int energyRestore;

        [Header("가격")]
        public int buyPrice;
        public int sellPrice;

        [Header("사운드")]
        public AudioClip useSound;
        public AudioClip pickupSound;
    }
}
