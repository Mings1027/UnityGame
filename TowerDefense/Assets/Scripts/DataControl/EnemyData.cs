using UnityEngine;

namespace DataControl
{
    [CreateAssetMenu(menuName = "EnemyData/EnemyData")]
    public class EnemyData : ScriptableObject
    {
        public GameObject EnemyPrefab => enemyPrefab;
        public byte StartSpawnWave => startSpawnWave;
        public float Speed => speed;
        public float AttackDelay => attackDelay;
        public ushort Damage => damage;
        public int Health => health;

        [SerializeField] private GameObject enemyPrefab;
        [SerializeField] private byte startSpawnWave;
        [Range(0, 5)] [SerializeField] private float speed;
        [SerializeField] private float attackDelay;
        [SerializeField] private ushort damage;
        [SerializeField] private int health;
    }
}