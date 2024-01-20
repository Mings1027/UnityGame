using DataControl.MonsterDataControl;
using UnityEngine;

namespace MonsterControl
{
    public class TransformMonster : MonoBehaviour
    {
        [field: SerializeField] public NormalMonsterData transformMonsterData { get; set; }
        [field: SerializeField] public byte spawnCount { get; set; }
    }
}