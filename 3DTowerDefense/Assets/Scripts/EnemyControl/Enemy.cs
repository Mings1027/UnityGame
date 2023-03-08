using System;
using System.Threading;
using AttackControl;
using Cysharp.Threading.Tasks;
using GameControl;
using UnityEngine;
using UnityEngine.AI;

namespace EnemyControl
{
    public abstract class Enemy : MonoBehaviour
    {
        private NavMeshAgent _nav;
        private CancellationTokenSource _cts;
        private bool _attackAble;
        private Collider[] _hits;
        private bool _isTargeting;

        protected Transform target;

        public Transform destination;
        public int damage;

        [SerializeField] private LayerMask unitLayer;
        [SerializeField] private float atkDelay;
        [SerializeField] private float atkRange;

        private void Awake()
        {
            _nav = GetComponent<NavMeshAgent>();
            _nav.stoppingDistance = atkRange;
            _hits = new Collider[3];
        }

        private void OnEnable()
        {
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
            _attackAble = true;

            InvokeRepeating(nameof(FindUnit), 0, 2f);
        }

        private void OnDisable()
        {
            _isTargeting = false;
            _cts?.Cancel();
            CancelInvoke();
            StackObjectPool.ReturnToPool(gameObject);
        }

        protected virtual void Update()
        {
            if (_isTargeting)
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
        }

        protected abstract void Attack();

        private async UniTaskVoid StartCoolDown()
        {
            _attackAble = false;
            await UniTask.Delay(TimeSpan.FromSeconds(atkDelay), cancellationToken: _cts.Token);
            _attackAble = true;
        }

        private void FindUnit()
        {
            var t = ObjectFinder.FindClosestObject(transform.position, atkRange, _hits, unitLayer);
            target = t.Item1;
            _isTargeting = t.Item2;
        }
    }
}