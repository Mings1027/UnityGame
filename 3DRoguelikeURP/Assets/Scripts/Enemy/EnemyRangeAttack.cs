using UnityEngine;

namespace Enemy
{
    public class EnemyRangeAttack : MonoBehaviour
    {
        [SerializeField] private float atkDelay;
        private bool isAttack;

        public void Attack()
        {
            if (isAttack) return;
            isAttack = true;
        }
    }
}