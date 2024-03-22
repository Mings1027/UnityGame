using CustomEnumControl;
using UnityEngine;

namespace DataControl.MonsterDataControl
{
    public abstract class MonsterData : ScriptableObject
    {
        [field: SerializeField] public MonsterPoolObjectKey monsterPoolObjectKey { get; private set; }
        [field: SerializeField, Range(0, 5)] public float speed { get; private set; }
        [field: SerializeField, Range(0, 10)] public float attackDelay { get; private set; }
        [field: SerializeField, Range(0, 5)] public byte maxDetectedCount { get; private set; }
        [field: SerializeField, Range(0, 10)] public byte attackRange { get; private set; }
        [field: SerializeField, Range(0, 10)] public byte sightRange { get; private set; }
        [field: SerializeField] public ushort damage { get; private set; }
        [field: SerializeField] public uint health { get; private set; }
        [field: SerializeField, Range(0, 10)] public byte baseTowerDamage { get; private set; }

        private void OnValidate()
        {
            if (attackRange >= sightRange)
            {
                sightRange = attackRange;
            }
        }
    }
}