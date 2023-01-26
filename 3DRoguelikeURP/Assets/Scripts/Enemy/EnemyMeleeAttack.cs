using DG.Tweening;
using Player;
using UnityEngine;

namespace Enemy
{
    public class EnemyMeleeAttack : MonoBehaviour
    {
        private int damage;
        [SerializeField] private Collider atkCollider;

        private void Awake()
        {
            atkCollider = GetComponent<Collider>();
        }

        public void Attack()
        {
            atkCollider.enabled = true;
            DOVirtual.DelayedCall(0.5f, () => atkCollider.enabled = false);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            other.GetComponent<PlayerHealth>().OnDamage(damage, transform);
            atkCollider.enabled = false;
        }
    }
}