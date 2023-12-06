using CustomEnumControl;
using UnityEngine;

namespace UnitControl.EnemyControl
{
    public class TransformEnemy : MonoBehaviour
    {
        public EnemyPoolObjectKey EnemyPoolObjectKey => enemyPoolObjectKey;
        public byte SpawnCount => spawnCount;
        [SerializeField] private EnemyPoolObjectKey enemyPoolObjectKey;
        [SerializeField] private byte spawnCount;
    }
}