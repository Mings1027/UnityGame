using UnityEngine;

namespace DataControl
{
    public abstract class MonsterData : ScriptableObject
    {
        public bool IsTransformingMonster => isTransformingMonster;
        public float Speed => speed;
        public float AttackDelay => attackDelay;
        public ushort Damage => damage;
        public int Health => health;

        [SerializeField] private bool isTransformingMonster;
        [Range(0, 5)] [SerializeField] private float speed;
        [SerializeField] private float attackDelay;
        [SerializeField] private ushort damage;
        [SerializeField] private int health;
    }
}