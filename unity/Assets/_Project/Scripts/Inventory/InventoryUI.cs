using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using GoldenAge.Combat;

namespace GoldenAge.Inventory
{
    /// <summary>
    /// 인벤토리 UI 관리
    /// </summary>
    public class InventoryUI : MonoBehaviour
    {
        [Header("UI 참조")]
        [SerializeField] private GameObject inventoryPanel;
        [SerializeField] private Transform slotsContainer;
        [SerializeField] private GameObject slotPrefab;

        [Header("아이템 정보")]
        [SerializeField] private GameObject itemInfoPanel;
        [SerializeField] private Image itemIcon;
        [SerializeField] private TextMeshProUGUI itemNameText;
        [SerializeField] private TextMeshProUGUI itemDescText;
        [SerializeField] private TextMeshProUGUI itemTypeText;
        [SerializeField] private Button useButton;
        [SerializeField] private Button dropButton;

        [Header("설정")]
        [SerializeField] private KeyCode toggleKey = KeyCode.I;

        private InventorySystem inventory;
        private List<InventorySlotUI> slotUIs = new List<InventorySlotUI>();
        private int selectedSlotIndex = -1;
        private bool isOpen = false;

        private void Start()
        {
            inventory = GetComponent<InventorySystem>();
            if (inventory == null)
            {
                inventory = FindObjectOfType<InventorySystem>();
            }

            if (inventory != null)
            {
                inventory.OnInventoryChanged += RefreshUI;
            }

            // 초기 슬롯 UI 생성
            CreateSlotUIs();

            // 초기 상태: 닫힘
            if (inventoryPanel != null)
                inventoryPanel.SetActive(false);

            if (itemInfoPanel != null)
                itemInfoPanel.SetActive(false);

            // 버튼 이벤트
            if (useButton != null)
                useButton.onClick.AddListener(UseSelectedItem);
            if (dropButton != null)
                dropButton.onClick.AddListener(DropSelectedItem);
        }

        private void Update()
        {
            if (Input.GetKeyDown(toggleKey))
            {
                ToggleInventory();
            }
        }

        private void OnDestroy()
        {
            if (inventory != null)
            {
                inventory.OnInventoryChanged -= RefreshUI;
            }
        }

        public void ToggleInventory()
        {
            isOpen = !isOpen;

            if (inventoryPanel != null)
                inventoryPanel.SetActive(isOpen);

            if (isOpen)
            {
                RefreshUI();
                Time.timeScale = 0f; // 일시정지
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Time.timeScale = 1f;
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                ClearSelection();
            }
        }

        private void CreateSlotUIs()
        {
            if (slotsContainer == null || slotPrefab == null) return;

            // 기존 슬롯 제거
            foreach (Transform child in slotsContainer)
            {
                Destroy(child.gameObject);
            }
            slotUIs.Clear();

            // 새 슬롯 생성
            int slotCount = inventory != null ? inventory.MaxSlots : 20;
            for (int i = 0; i < slotCount; i++)
            {
                GameObject slotObj = Instantiate(slotPrefab, slotsContainer);
                InventorySlotUI slotUI = slotObj.GetComponent<InventorySlotUI>();

                if (slotUI == null)
                {
                    slotUI = slotObj.AddComponent<InventorySlotUI>();
                }

                int index = i;
                slotUI.Initialize(index, () => SelectSlot(index));
                slotUIs.Add(slotUI);
            }
        }

        public void RefreshUI()
        {
            if (inventory == null) return;

            for (int i = 0; i < slotUIs.Count && i < inventory.Slots.Count; i++)
            {
                InventorySlot slot = inventory.Slots[i];
                slotUIs[i].UpdateSlot(slot);
                slotUIs[i].SetSelected(i == selectedSlotIndex);
            }
        }

        private void SelectSlot(int index)
        {
            selectedSlotIndex = index;
            RefreshUI();
            ShowItemInfo(index);
        }

        private void ClearSelection()
        {
            selectedSlotIndex = -1;
            if (itemInfoPanel != null)
                itemInfoPanel.SetActive(false);
        }

        private void ShowItemInfo(int slotIndex)
        {
            if (itemInfoPanel == null) return;

            if (slotIndex < 0 || slotIndex >= inventory.Slots.Count)
            {
                itemInfoPanel.SetActive(false);
                return;
            }

            InventorySlot slot = inventory.Slots[slotIndex];
            if (slot.IsEmpty)
            {
                itemInfoPanel.SetActive(false);
                return;
            }

            ItemData item = slot.Item;
            itemInfoPanel.SetActive(true);

            if (itemIcon != null && item.icon != null)
                itemIcon.sprite = item.icon;

            if (itemNameText != null)
                itemNameText.text = item.itemName;

            if (itemDescText != null)
                itemDescText.text = item.description;

            if (itemTypeText != null)
                itemTypeText.text = GetItemTypeText(item);

            // 사용 버튼 활성화 여부
            if (useButton != null)
                useButton.interactable = item.itemType == ItemType.Consumable;
        }

        private string GetItemTypeText(ItemData item)
        {
            string typeStr = item.itemType switch
            {
                ItemType.Consumable => "소비",
                ItemType.Equipment => "장비",
                ItemType.Quest => "퀘스트",
                ItemType.Material => "재료",
                ItemType.Key => "열쇠",
                _ => "기타"
            };

            string rarityStr = item.rarity switch
            {
                ItemRarity.Common => "<color=#FFFFFF>일반</color>",
                ItemRarity.Uncommon => "<color=#00FF00>고급</color>",
                ItemRarity.Rare => "<color=#0080FF>희귀</color>",
                ItemRarity.Epic => "<color=#9900FF>영웅</color>",
                ItemRarity.Legendary => "<color=#FF8000>전설</color>",
                _ => "일반"
            };

            return $"{typeStr} | {rarityStr}";
        }

        private void UseSelectedItem()
        {
            if (selectedSlotIndex >= 0 && inventory != null)
            {
                inventory.UseItem(selectedSlotIndex);
                ShowItemInfo(selectedSlotIndex);
            }
        }

        private void DropSelectedItem()
        {
            if (selectedSlotIndex >= 0 && inventory != null)
            {
                InventorySlot slot = inventory.Slots[selectedSlotIndex];
                if (!slot.IsEmpty)
                {
                    // TODO: 월드에 아이템 드롭 생성
                    inventory.RemoveItem(slot.Item, 1);
                    ShowItemInfo(selectedSlotIndex);
                }
            }
        }

        public void SortInventory()
        {
            inventory?.SortInventory();
        }
    }

    /// <summary>
    /// 개별 슬롯 UI
    /// </summary>
    public class InventorySlotUI : MonoBehaviour
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI countText;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image selectedBorder;

        private Button button;
        private int slotIndex;
        private System.Action onClick;

        public void Initialize(int index, System.Action clickAction)
        {
            slotIndex = index;
            onClick = clickAction;

            button = GetComponent<Button>();
            if (button == null)
                button = gameObject.AddComponent<Button>();

            button.onClick.AddListener(() => onClick?.Invoke());

            // 컴포넌트 자동 찾기
            if (iconImage == null)
                iconImage = transform.Find("Icon")?.GetComponent<Image>();
            if (countText == null)
                countText = transform.Find("Count")?.GetComponent<TextMeshProUGUI>();
            if (selectedBorder == null)
                selectedBorder = transform.Find("Selected")?.GetComponent<Image>();
        }

        public void UpdateSlot(InventorySlot slot)
        {
            if (slot == null || slot.IsEmpty)
            {
                if (iconImage != null)
                {
                    iconImage.enabled = false;
                    iconImage.sprite = null;
                }
                if (countText != null)
                    countText.text = "";
            }
            else
            {
                if (iconImage != null)
                {
                    iconImage.enabled = true;
                    iconImage.sprite = slot.Item.icon;
                }
                if (countText != null)
                {
                    countText.text = slot.Count > 1 ? slot.Count.ToString() : "";
                }
            }
        }

        public void SetSelected(bool selected)
        {
            if (selectedBorder != null)
                selectedBorder.enabled = selected;
        }
    }
}
