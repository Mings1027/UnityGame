using System;
using System.Threading;
using AttackControl;
using UnityEngine;
using UnityEngine.AI;

namespace EnemyControl
{
    public abstract class Enemy : MonoBehaviour
    {
        protected TargetFinder targetFinder;
        private NavMeshAgent _nav;

        private CancellationTokenSource _cts;

        public Transform destination;
        public int damage;

        protected virtual void Awake()
        {
            targetFinder = GetComponent<TargetFinder>();
            _nav = GetComponent<NavMeshAgent>();
        }

        private void OnEnable()
        {
            _cts?.Dispose();
            _cts = new CancellationTokenSource();

            InvokeRepeating(nameof(FindUnit), 0, 0.5f);
        }

        private void OnDisable()
        {
            _cts?.Cancel();
            CancelInvoke();
        }

        protected abstract void Attack();

        private void FindUnit()
        {
            if (targetFinder.IsTargeting)
            {
                if (targetFinder.attackAble &&
                    Vector3.Distance(transform.position, targetFinder.Target.position) <= targetFinder.AtkRange)
                {
                    _nav.isStopped = true;
                    Attack();
                    targetFinder.StartCoolDown().Forget();
                }
                else
                {
                    _nav.SetDestination(targetFinder.Target.position);
                }
            }
            else
            {
                if (_nav.isStopped) _nav.isStopped = false;
                _nav.SetDestination(destination.position);
                if (Vector3.Distance(transform.position, destination.position) <= _nav.stoppingDistance)
                    gameObject.SetActive(false);
            }
        }
    }
}