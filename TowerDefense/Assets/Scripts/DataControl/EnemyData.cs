using UnityEngine;

namespace DataControl
{
    public abstract class EnemyData : ScriptableObject
    {
        public byte StartSpawnWave => startSpawnWave;
        public float Speed => speed;
        public float AttackDelay => attackDelay;
        public ushort Damage => damage;
        public int Health => health;

        [SerializeField] private byte startSpawnWave;
        [Range(0, 5)] [SerializeField] private float speed;
        [SerializeField] private float attackDelay;
        [SerializeField] private ushort damage;
        [SerializeField] private int health;
    }
}