using System;
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
        private CancellationTokenSource _cts;
        private bool _attackAble;
        private RaycastHit[] _hits;

        public bool isTargeting;
        public Transform target;
        public Transform destination;
        public int damage;

        [SerializeField] private float atkDelay;
        [SerializeField] private float atkRange;

        private void Awake()
        {
            _nav = GetComponent<NavMeshAgent>();
            _nav.stoppingDistance = atkRange;
        }

        private void OnEnable()
        {
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
            _attackAble = true;
        }

        private void OnDisable()
        {
            isTargeting = false;
            _cts?.Cancel();
            CancelInvoke();
            StackObjectPool.ReturnToPool(gameObject);
        }

        protected virtual void Update()
        {
            if (isTargeting)
            {
                if (_attackAble && _nav.remainingDistance <= _nav.stoppingDistance)
                {
                    Attack();
                    StartCoolDown().Forget();
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

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, atkRange);
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position + transform.forward * 1, 1);
        }

        protected abstract void Attack();

        private async UniTaskVoid StartCoolDown()
        {
            _attackAble = false;
            await UniTask.Delay(TimeSpan.FromSeconds(atkDelay), cancellationToken: _cts.Token);
            _attackAble = true;
        }
    }
}