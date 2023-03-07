using System;
using System.Threading;
using AttackControl;
using Cysharp.Threading.Tasks;
using GameControl;
using UnityEngine;

namespace UnitControl
{
    public abstract class Unit : MonoBehaviour
    {
        private float _atkDelay;
        private CancellationTokenSource _cts;

        private float _atkRange;
        private Collider[] _hitCollider;
        private Vector3 _checkRangePoint;
        protected bool attackAble;
        protected int damage;

        protected bool isTargeting;

        public Transform target;

        [SerializeField] private LayerMask enemyLayer;
        [SerializeField] private LayerMask groundLayer;

        protected virtual void Awake()
        {
            _hitCollider = new Collider[3];
        }

        protected virtual void OnEnable()
        {
            attackAble = true;
            var r = ObjectFinder.HitObject(transform.position, Vector3.down, groundLayer);
            _checkRangePoint = r.point;

            _cts?.Dispose();
            _cts = new CancellationTokenSource();

            InvokeRepeating(nameof(FindEnemy), 0, 1f);
        }

        protected virtual void OnDisable()
        {
            CancelInvoke();
            _cts?.Cancel();
            StackObjectPool.ReturnToPool(gameObject);
        }
        protected virtual void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(_checkRangePoint, _atkRange);
        }
        public void UnitSetup(int unitDamage, float atkDelay, float attackRange)
        {
            damage = unitDamage;
            _atkDelay = atkDelay;
            _atkRange = attackRange;
        }

        protected abstract void Attack();

        private void FindEnemy()
        {
            var t = ObjectFinder.FindClosestObject(_checkRangePoint, _atkRange, _hitCollider, enemyLayer);
            target = t.Item1;
            isTargeting = t.Item2;
        }

        protected async UniTaskVoid StartCoolDown()
        {
            attackAble = false;
            await UniTask.Delay(TimeSpan.FromSeconds(_atkDelay), cancellationToken: _cts.Token);
            attackAble = true;
        }
    }
}