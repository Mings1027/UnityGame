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


        protected virtual void Awake()
        {
            targetFinder = GetComponent<TargetFinder>();
            _nav = GetComponent<NavMeshAgent>();
        }

        private void OnEnable()
        {
            _cts?.Dispose();
            _cts = new CancellationTokenSource();

            // InvokeRepeating(nameof(FindUnit), 0, 0.1f);
        }

        private void OnDisable()
        {
            _cts?.Cancel();
            CancelInvoke();
        }

        protected abstract void Attack();

        private void Update()
        {
            if (targetFinder.IsTargeting)
            {
                if (Vector3.Distance(transform.position, targetFinder.Target.position) <= targetFinder.AtkRange)
                {
                    if (targetFinder.attackAble)
                    {
                        _nav.isStopped = true;
                        Attack();
                        targetFinder.StartCoolDown();
                    }
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