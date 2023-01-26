using UnityEngine;
using UnityEngine.Events;
using static UnityEngine.LayerMask;
using static UnityEngine.Physics2D;

namespace Enemy
{
    public class EnemyAI : MonoBehaviour
    {
        [SerializeField] private UnityEvent onEnableChase;

        [SerializeField] private UnityEvent<float> onRandomMove;
        [SerializeField] private UnityEvent onAttack;
        [SerializeField] private UnityEvent<Vector3> onFlip;

        [SerializeField] private Transform player;
        [SerializeField] private float chaseRange, attackRange;

        private void Update()
        {
            var playerPosition = player.position;
            var transformPosition = transform.position;
            var distance = Vector2.Distance(playerPosition, transformPosition);

            if (distance < chaseRange)
            {
                if (distance < attackRange)
                {
                    onAttack?.Invoke();
                }
                else
                {
                    onEnableChase?.Invoke();
                }
            }
            else
            {
                onRandomMove?.Invoke(attackRange);
            }

            onFlip?.Invoke(playerPosition);
        }

        public void DetectCollidors()
        {
            var hit = OverlapCircle(transform.position, attackRange, GetMask("Player"));
            if (!hit) return;
            var value = Random.Range(1, 6);
            hit.GetComponent<KnockBack>().thrust = value;
            hit.GetComponent<Status>().GetHit(value, transform.gameObject);
        }


        private void OnDrawGizmosSelected()
        {
            var position = transform.position;
            Gizmos.DrawWireSphere(position, chaseRange);
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(position, attackRange);
        }
    }
}