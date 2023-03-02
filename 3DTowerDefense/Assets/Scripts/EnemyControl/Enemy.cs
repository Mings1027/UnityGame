using System.Threading;
using Cysharp.Threading.Tasks;
using GameControl;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

namespace EnemyControl
{
    public abstract class Enemy : MonoBehaviour
    {
        private NavMeshAgent _nav;
        private bool _isTargeting;

        protected Transform target;
        protected CancellationTokenSource cts;
        protected bool attackAble;

        protected float AtkDelay
        {
            get => atkDelay;
            set => atkDelay = value;
        }

        public Transform destination;
        public int damage;

        [SerializeField] private float atkDelay;
        [SerializeField] private float range;
        [SerializeField] private LayerMask attackAbleLayer;
        [SerializeField] private Collider[] targets;


        private void Awake()
        {
            _nav = GetComponent<NavMeshAgent>();
            targets = new Collider[5];
        }

        private void OnEnable()
        {
            cts?.Dispose();
            cts = new CancellationTokenSource();
            attackAble = true;
            InvokeRepeating(nameof(UpdateTarget), 0f, 0.5f);
        }

        private void OnDisable()
        {
            cts?.Cancel();
            CancelInvoke();
            StackObjectPool.ReturnToPool(gameObject);
        }

        protected virtual void Update()
        {
            if (_isTargeting)
            {
                _nav.SetDestination(target.position);
                if (_nav.remainingDistance <= _nav.stoppingDistance)
                {
                    if (!attackAble) return;
                    Attack();
                    StartCoolDown().Forget();
                }
            }
            else
            {
                _nav.SetDestination(destination.position);
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, range);
        }

        protected abstract void Attack();

        protected virtual async UniTaskVoid StartCoolDown()
        {
        }

        private void UpdateTarget()
        {
            var size = Physics.OverlapSphereNonAlloc(transform.position, range, targets, attackAbleLayer);
            var shortestDistance = Mathf.Infinity;
            Transform nearestTarget = null;
            for (var i = 0; i < size; i++)
            {
                var distanceToUnit = Vector3.Distance(transform.position, targets[i].transform.position);
                if (distanceToUnit < shortestDistance)
                {
                    shortestDistance = distanceToUnit;
                    nearestTarget = targets[i].transform;
                }
            }

            target = nearestTarget != null && shortestDistance <= range ? nearestTarget : null;
            _isTargeting = target != null;
        }
    }
}