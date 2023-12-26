using UnityEngine;

namespace DataControl
{
    [CreateAssetMenu(menuName = "MonsterData/Boss Monster Data")]
    public class BossMonsterData : MonsterData
    {
        public GameObject EnemyPrefab => enemyPrefab;
        public ushort DroppedGold => droppedGold;
        
        [SerializeField] private GameObject enemyPrefab;
        [SerializeField] private ushort droppedGold;
    }
}