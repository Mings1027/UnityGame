using UnityEngine;

namespace DataControl.MonsterDataControl
{
    [CreateAssetMenu(menuName = "MonsterData/Normal Monster Data")]
    public class NormalMonsterData : MonsterData
    {
        [field: SerializeField] public byte startSpawnWave { get; private set; }
    }
}