using UnityEngine;
using System;
using System.Collections.Generic;
using GoldenAge.Combat;

namespace GoldenAge.Inventory
{
    /// <summary>
    /// 인벤토리 관리 시스템
    /// </summary>
    public class InventorySystem : MonoBehaviour
    {
        [Header("설정")]
        [SerializeField] private int maxSlots = 20;
        [SerializeField] private int maxStackSize = 99;

        // 인벤토리 데이터
        private List<InventorySlot> slots = new List<InventorySlot>();

        // 이벤트
        public event Action<InventorySlot> OnItemAdded;
        public event Action<InventorySlot> OnItemRemoved;
        public event Action<InventorySlot> OnItemUsed;
        public event Action OnInventoryChanged;

        public int MaxSlots => maxSlots;
        public int UsedSlots => slots.FindAll(s => !s.IsEmpty).Count;
        public List<InventorySlot> Slots => slots;

        private void Awake()
        {
            // 슬롯 초기화
            for (int i = 0; i < maxSlots; i++)
            {
                slots.Add(new InventorySlot());
            }
        }

        /// <summary>
        /// 아이템 추가
        /// </summary>
        public bool AddItem(ItemData item, int amount = 1)
        {
            if (item == null || amount <= 0) return false;

            int remaining = amount;

            // 1. 기존 스택에 추가 시도
            if (item.maxStack > 1)
            {
                foreach (var slot in slots)
                {
                    if (slot.Item == item && slot.Count < item.maxStack)
                    {
                        int canAdd = Mathf.Min(remaining, item.maxStack - slot.Count);
                        slot.Count += canAdd;
                        remaining -= canAdd;

                        OnItemAdded?.Invoke(slot);

                        if (remaining <= 0) break;
                    }
                }
            }

            // 2. 새 슬롯에 추가
            while (remaining > 0)
            {
                InventorySlot emptySlot = slots.Find(s => s.IsEmpty);
                if (emptySlot == null)
                {
                    Debug.LogWarning("[Inventory] 인벤토리가 가득 찼습니다!");
                    return false;
                }

                int toAdd = Mathf.Min(remaining, item.maxStack);
                emptySlot.Item = item;
                emptySlot.Count = toAdd;
                remaining -= toAdd;

                OnItemAdded?.Invoke(emptySlot);
            }

            OnInventoryChanged?.Invoke();
            Debug.Log($"[Inventory] {item.itemName} x{amount} 추가됨");
            return true;
        }

        /// <summary>
        /// 아이템 제거
        /// </summary>
        public bool RemoveItem(ItemData item, int amount = 1)
        {
            if (item == null || amount <= 0) return false;

            int totalCount = GetItemCount(item);
            if (totalCount < amount)
            {
                Debug.LogWarning($"[Inventory] {item.itemName}이 부족합니다.");
                return false;
            }

            int remaining = amount;

            // 뒤에서부터 제거 (부분 스택 먼저)
            for (int i = slots.Count - 1; i >= 0 && remaining > 0; i--)
            {
                if (slots[i].Item == item)
                {
                    int toRemove = Mathf.Min(remaining, slots[i].Count);
                    slots[i].Count -= toRemove;
                    remaining -= toRemove;

                    if (slots[i].Count <= 0)
                    {
                        OnItemRemoved?.Invoke(slots[i]);
                        slots[i].Clear();
                    }
                }
            }

            OnInventoryChanged?.Invoke();
            Debug.Log($"[Inventory] {item.itemName} x{amount} 제거됨");
            return true;
        }

        /// <summary>
        /// 아이템 사용
        /// </summary>
        public bool UseItem(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= slots.Count) return false;

            InventorySlot slot = slots[slotIndex];
            if (slot.IsEmpty) return false;

            ItemData item = slot.Item;

            // 소비 아이템만 사용 가능
            if (item.itemType != ItemType.Consumable)
            {
                Debug.Log($"[Inventory] {item.itemName}은 사용할 수 없는 아이템입니다.");
                return false;
            }

            // 효과 적용
            ApplyItemEffect(item);

            // 수량 감소
            slot.Count--;
            if (slot.Count <= 0)
            {
                slot.Clear();
            }

            OnItemUsed?.Invoke(slot);
            OnInventoryChanged?.Invoke();

            Debug.Log($"[Inventory] {item.itemName} 사용됨");
            return true;
        }

        private void ApplyItemEffect(ItemData item)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null) return;

            var stats = player.GetComponent<Player.PlayerStats>();
            if (stats == null) return;

            if (item.healAmount > 0)
            {
                stats.Heal(item.healAmount);
            }

            if (item.energyAmount > 0)
            {
                stats.RestoreEnergy(item.energyAmount);
            }
        }

        /// <summary>
        /// 아이템 보유 수량
        /// </summary>
        public int GetItemCount(ItemData item)
        {
            int count = 0;
            foreach (var slot in slots)
            {
                if (slot.Item == item)
                {
                    count += slot.Count;
                }
            }
            return count;
        }

        /// <summary>
        /// 아이템 보유 여부
        /// </summary>
        public bool HasItem(ItemData item, int amount = 1)
        {
            return GetItemCount(item) >= amount;
        }

        /// <summary>
        /// 특정 타입 아이템 목록
        /// </summary>
        public List<InventorySlot> GetItemsByType(ItemType type)
        {
            return slots.FindAll(s => !s.IsEmpty && s.Item.itemType == type);
        }

        /// <summary>
        /// 인벤토리 정렬
        /// </summary>
        public void SortInventory()
        {
            // 빈 슬롯이 아닌 것만 추출
            List<InventorySlot> filledSlots = slots.FindAll(s => !s.IsEmpty);

            // 타입, 레어리티, 이름 순으로 정렬
            filledSlots.Sort((a, b) =>
            {
                int typeCompare = a.Item.itemType.CompareTo(b.Item.itemType);
                if (typeCompare != 0) return typeCompare;

                int rarityCompare = b.Item.rarity.CompareTo(a.Item.rarity);
                if (rarityCompare != 0) return rarityCompare;

                return a.Item.itemName.CompareTo(b.Item.itemName);
            });

            // 슬롯 재배치
            for (int i = 0; i < slots.Count; i++)
            {
                if (i < filledSlots.Count)
                {
                    slots[i].Item = filledSlots[i].Item;
                    slots[i].Count = filledSlots[i].Count;
                }
                else
                {
                    slots[i].Clear();
                }
            }

            OnInventoryChanged?.Invoke();
            Debug.Log("[Inventory] 정렬 완료");
        }

        /// <summary>
        /// 인벤토리 초기화
        /// </summary>
        public void ClearInventory()
        {
            foreach (var slot in slots)
            {
                slot.Clear();
            }
            OnInventoryChanged?.Invoke();
        }
    }

    [Serializable]
    public class InventorySlot
    {
        public ItemData Item;
        public int Count;

        public bool IsEmpty => Item == null || Count <= 0;

        public void Clear()
        {
            Item = null;
            Count = 0;
        }
    }
}
