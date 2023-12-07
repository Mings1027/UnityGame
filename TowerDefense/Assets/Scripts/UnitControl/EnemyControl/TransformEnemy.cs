using CustomEnumControl;
using UnityEngine;
using UnityEngine.Serialization;

namespace UnitControl.EnemyControl
{
    public class TransformEnemy : MonoBehaviour
    {
        public MonsterPoolObjectKey MonsterPoolObjectKey => monsterPoolObjectKey;
        public byte SpawnCount => spawnCount;
        [FormerlySerializedAs("enemyPoolObjectKey")] [SerializeField] private MonsterPoolObjectKey monsterPoolObjectKey;
        [SerializeField] private byte spawnCount;
    }
}