using UnityEngine;

namespace DataControl
{
    [CreateAssetMenu(menuName = "EnemyData/BossData")]
    public class BossData : EnemyData
    {
        public ushort DroppedGold => droppedGold;
        [SerializeField] private ushort droppedGold;
    }
}