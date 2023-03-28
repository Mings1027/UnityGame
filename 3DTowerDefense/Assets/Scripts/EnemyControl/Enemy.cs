using AttackControl;
using UnityEngine;
using UnityEngine.AI;

namespace EnemyControl
{
    public abstract class Enemy : MonoBehaviour
    {
        protected TargetFinder targetFinder;
        private NavMeshAgent _nav;

        public Transform destination;


        protected virtual void Awake()
        {
            targetFinder = GetComponent<TargetFinder>();
            _nav = GetComponent<NavMeshAgent>();
        }

        private void OnDisable()
        {
            CancelInvoke();
        }

        protected abstract void Attack();

        private void Update()
        {
            if (targetFinder.IsTargeting)
            {
                if (Vector3.Distance(transform.position, targetFinder.Target.position) <= _nav.stoppingDistance)
                {
                    if (!targetFinder.attackAble) return;
                    _nav.isStopped = true;
                    Attack();
                    targetFinder.StartCoolDown();
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