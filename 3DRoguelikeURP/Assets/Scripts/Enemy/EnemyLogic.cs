using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using Random = UnityEngine.Random;

namespace Enemy
{
    public class EnemyLogic : MonoBehaviour
    {
        private Transform player;
        private NavMeshAgent nav;

        [Header("Chase")] [SerializeField] [Range(0, 30)]
        private float chaseRange;

        [Header("Patrol")] [SerializeField] private Transform centerPoint;
        [SerializeField] [Range(0, 30)] private float patrolRange;

        [Header("Attack")] [SerializeField] private float lookSmooth;
        [SerializeField] private float atkDelay;
        [SerializeField] private UnityEvent attackEvent;
        private float atkTime;
        private bool isAttack;

        private void OnEnable()
        {
            player = GameObject.FindGameObjectWithTag("Player").transform;
            nav = GetComponent<NavMeshAgent>();
            atkTime = atkDelay;
        }

        private void Update()
        {
            var dis = Distance();
            if (dis <= chaseRange)
            {
                if (dis <= nav.stoppingDistance)
                {
                    FaceTarget();
                    Attack();
                }
                else ChasePlayer();
            }
            else Patrol();
        }

        private float Distance()
        {
            return Vector3.Distance(transform.position, player.position);
        }

        private void FaceTarget()
        {
            if (isAttack) return;
            var dir = (player.position - transform.position).normalized;
            var lookRotation = Quaternion.LookRotation(new Vector3(dir.x, 0, dir.z));
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * lookSmooth);
        }

        private void Attack()
        {
            if (atkTime > 0)
            {
                isAttack = false;
                atkTime -= Time.deltaTime;
            }
            else
            {
                isAttack = true;
                attackEvent?.Invoke();
                atkTime = atkDelay;
            }
        }

        private void ChasePlayer()
        {
            nav.SetDestination(player.position);
        }

        private void Patrol()
        {
            if (nav.remainingDistance > nav.stoppingDistance) return;
            if (RandomPoint(centerPoint.position, patrolRange, out var point)) nav.SetDestination(point);
        }

        private static bool RandomPoint(Vector3 center, float range, out Vector3 result)
        {
            var randomPoint = center + Random.insideUnitSphere * range;
            result = NavMesh.SamplePosition(randomPoint, out var hit, 1.0f, NavMesh.AllAreas)
                ? hit.position
                : Vector3.zero;
            return result == hit.position;
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawWireSphere(transform.position, chaseRange);
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(centerPoint.position, patrolRange);
        }
    }
}