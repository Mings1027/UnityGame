using CustomEnumControl;
using UnityEngine;

namespace DataControl
{
    [CreateAssetMenu]
    public class EnemyData : ScriptableObject
    {
        public EnemyPoolObjectKey EnemyKey => enemyKey;
        public float Speed => speed;
        public ushort EnemyCoin => enemyCoin;
        public byte AttackDelay => attackDelay;
        public ushort Damage => damage;
        public int Health => health;

        [SerializeField] private EnemyPoolObjectKey enemyKey;
        [Range(0, 5)] [SerializeField] private float speed;
        [SerializeField] private ushort enemyCoin;
        [SerializeField] private byte attackDelay;
        [SerializeField] private ushort damage;
        [SerializeField] private int health;
    }
}