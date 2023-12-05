using CustomEnumControl;
using UnityEngine;

namespace DataControl
{
    [CreateAssetMenu(menuName = "EnemyData/MonsterData")]
    public class MonsterData : EnemyData
    {
        public EnemyPoolObjectKey EnemyKey => enemyKey;
        [SerializeField] private EnemyPoolObjectKey enemyKey;
    }
}