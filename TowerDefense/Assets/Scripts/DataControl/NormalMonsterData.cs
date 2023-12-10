using CustomEnumControl;
using UnityEngine;
using UnityEngine.Serialization;

namespace DataControl
{
    [CreateAssetMenu(menuName = "MonsterData/Normal Monster Data")]
    public class NormalMonsterData : MonsterData
    {
        public byte StartSpawnWave => startSpawnWave;
        public MonsterPoolObjectKey MonsterPoolObjectKey => monsterPoolObjectKey;
        [SerializeField] private MonsterPoolObjectKey monsterPoolObjectKey;
        [SerializeField] private byte startSpawnWave;
    }
}