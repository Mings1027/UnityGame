using CustomEnumControl;
using UnityEngine;
using UnityEngine.Serialization;

namespace DataControl
{
    [CreateAssetMenu(menuName = "MonsterData/Normal Monster Data")]
    public class NormalMonsterData : MonsterData
    {
        public MonsterPoolObjectKey monsterPoolObjectKey;
    }
}