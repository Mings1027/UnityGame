using DataControl;
using UnityEngine;

namespace UnitControl.EnemyControl
{
    public class TransformMonster : MonoBehaviour
    {
        public byte SpawnCount => spawnCount;
        public NormalMonsterData TransformMonsterData => transformMonsterData;

        [SerializeField] private NormalMonsterData transformMonsterData;
        [SerializeField] private byte spawnCount;
    }
}