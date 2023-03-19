using System.Threading;
using AttackControl;
using GameControl;
using UnityEngine;
using UnityEngine.AI;

namespace EnemyControl
{
    public abstract class Enemy : MonoBehaviour
    {
        private TargetFinder _targetFinder;
        private NavMeshAgent _nav;

        private CancellationTokenSource _cts;

        private bool _isTargeting;

        protected Transform target;

        public Transform destination;
        public int damage;

        private void Awake()
        {
            _targetFinder = GetComponent<TargetFinder>();
            _targetFinder.colliderSize = 2;
            _nav = GetComponent<NavMeshAgent>();
        }

        private void OnEnable()
        {
            _cts?.Dispose();
            _cts = new CancellationTokenSource();

            InvokeRepeating(nameof(FindUnit), 0, 1f);
        }

        private void OnDisable()
        {
            _isTargeting = false;
            _cts?.Cancel();
            CancelInvoke();
        }

        protected virtual void Update()
        {
            if (_isTargeting)
            {
                if (_targetFinder.attackAble &&
                    Vector3.Distance(transform.position, target.position) <= _targetFinder.AtkRange)
                {
                    _nav.isStopped = true;
                    Attack();
                    _targetFinder.StartCoolDown().Forget();
                }
                else
                {
                    _nav.SetDestination(target.position);
                }
            }
            else
            {
                _nav.SetDestination(destination.position);
                if (Vector3.Distance(transform.position, destination.position) <= _nav.stoppingDistance)
                    gameObject.SetActive(false);
            }
        }

        protected abstract void Attack();

        private void FindUnit()
        {
            var t = _targetFinder.FindClosestTarget();

            target = t.Item1;
            _isTargeting = t.Item2;
        }
    }
}