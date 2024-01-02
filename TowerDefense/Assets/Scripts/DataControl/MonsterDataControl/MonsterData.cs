using UnityEngine;

namespace DataControl.MonsterDataControl
{
    public abstract class MonsterData : ScriptableObject
    {
        public float Speed => speed;
        public float AttackDelay => attackDelay;
        public ushort Damage => damage;
        public int Health => health;

        [Range(0, 5)] [SerializeField] private float speed;
        [SerializeField] private float attackDelay;
        [SerializeField] private ushort damage;
        [SerializeField] private int health;
    }
}