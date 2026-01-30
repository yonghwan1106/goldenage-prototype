using UnityEngine;

namespace GoldenAge.Combat
{
    /// <summary>
    /// 스폰 포인트 컴포넌트
    /// </summary>
    public class SpawnPoint : MonoBehaviour
    {
        [Header("설정")]
        [SerializeField] private SpawnPointType pointType = SpawnPointType.Enemy;
        [SerializeField] private float spawnRadius = 1f;
        [SerializeField] private bool isActive = true;

        [Header("시각화")]
        [SerializeField] private Color gizmoColor = Color.red;

        public SpawnPointType PointType => pointType;
        public bool IsActive => isActive;

        public Vector3 GetSpawnPosition()
        {
            Vector2 randomOffset = Random.insideUnitCircle * spawnRadius;
            return transform.position + new Vector3(randomOffset.x, 0, randomOffset.y);
        }

        public void SetActive(bool active)
        {
            isActive = active;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = isActive ? gizmoColor : Color.gray;
            Gizmos.DrawWireSphere(transform.position, spawnRadius);

            // 방향 표시
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(transform.position, transform.forward * 2f);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = gizmoColor;
            Gizmos.DrawSphere(transform.position, 0.3f);
        }
    }

    public enum SpawnPointType
    {
        Player,
        Enemy,
        NPC,
        Item
    }
}
