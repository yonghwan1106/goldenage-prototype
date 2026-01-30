using UnityEngine;
using GoldenAge.Combat;
using GoldenAge.Player;

namespace GoldenAge.Environment
{
    /// <summary>
    /// 줍기 가능한 아이템
    /// </summary>
    public class PickupItem : MonoBehaviour, IInteractable
    {
        [Header("아이템 정보")]
        [SerializeField] private ItemData itemData;
        [SerializeField] private int quantity = 1;

        [Header("시각 효과")]
        [SerializeField] private bool rotateItem = true;
        [SerializeField] private float rotationSpeed = 50f;
        [SerializeField] private bool floatItem = true;
        [SerializeField] private float floatHeight = 0.2f;
        [SerializeField] private float floatSpeed = 2f;

        [Header("픽업 효과")]
        [SerializeField] private GameObject pickupVFX;
        [SerializeField] private AudioClip pickupSound;

        private Vector3 startPosition;
        private float floatTimer;

        private void Start()
        {
            startPosition = transform.position;
        }

        private void Update()
        {
            if (rotateItem)
            {
                transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
            }

            if (floatItem)
            {
                floatTimer += Time.deltaTime * floatSpeed;
                float yOffset = Mathf.Sin(floatTimer) * floatHeight;
                transform.position = startPosition + new Vector3(0, yOffset, 0);
            }
        }

        public string GetInteractionPrompt()
        {
            if (itemData == null) return "줍기";
            return $"줍기: {itemData.itemName}" + (quantity > 1 ? $" x{quantity}" : "");
        }

        public void OnInteract(GameObject interactor)
        {
            if (itemData == null)
            {
                Debug.LogWarning("[Pickup] 아이템 데이터가 없습니다.");
                return;
            }

            // 인벤토리에 추가 (인벤토리 시스템 연동)
            // 현재는 즉시 효과 적용
            ApplyItemEffect(interactor);

            // 픽업 효과
            PlayPickupEffects();

            // 아이템 제거
            Destroy(gameObject);
        }

        private void ApplyItemEffect(GameObject target)
        {
            PlayerStats stats = target.GetComponent<PlayerStats>();
            if (stats == null) return;

            switch (itemData.itemType)
            {
                case ItemType.Consumable:
                    if (itemData.healAmount > 0)
                        stats.Heal(itemData.healAmount * quantity);
                    if (itemData.energyAmount > 0)
                        stats.RestoreEnergy(itemData.energyAmount * quantity);
                    break;

                case ItemType.Quest:
                    Debug.Log($"[Pickup] 퀘스트 아이템 획득: {itemData.itemName}");
                    break;

                default:
                    Debug.Log($"[Pickup] 아이템 획득: {itemData.itemName} x{quantity}");
                    break;
            }
        }

        private void PlayPickupEffects()
        {
            // VFX
            if (pickupVFX != null)
            {
                Instantiate(pickupVFX, transform.position, Quaternion.identity);
            }

            // 사운드
            if (pickupSound != null)
            {
                AudioSource.PlayClipAtPoint(pickupSound, transform.position);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, 0.3f);
        }
    }
}
