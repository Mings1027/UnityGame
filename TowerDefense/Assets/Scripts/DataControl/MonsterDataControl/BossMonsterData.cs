using UnityEngine;

namespace DataControl.MonsterDataControl
{
    [CreateAssetMenu(menuName = "MonsterData/Boss Monster Data")]
    public class BossMonsterData : MonsterData
    {
        [field: SerializeField] public ushort droppedGold { get; private set; }
    }
}