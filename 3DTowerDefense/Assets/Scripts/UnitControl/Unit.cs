using System;
using System.Threading;
using AttackControl;
using Cysharp.Threading.Tasks;
using GameControl;
using UnityEngine;
using UnityEngine.Serialization;

namespace UnitControl
{
    public abstract class Unit : MonoBehaviour
    {
        private float _atkDelay;

        protected RaycastHit hit;
        protected float atkRange;
        protected Collider[] hitCollider;
        protected Vector3 checkRangePoint;
        protected bool attackAble;
        protected int damage;
        protected CancellationTokenSource cts;

        public bool IsTargeting { get; set; }

        public Transform target;

        protected LayerMask EnemyLayer => enemyLayer;
        protected LayerMask GroundLayer => groundLayer;

        [SerializeField] private LayerMask enemyLayer;
        [SerializeField] private LayerMask groundLayer;

        protected virtual void Awake()
        {
            hitCollider = new Collider[3];
        }

        protected virtual void OnEnable()
        {
            attackAble = true;
            var r = ObjectFinder.HitObject(transform.position, Vector3.down, groundLayer);
            checkRangePoint = r.point;

            cts?.Dispose();
            cts = new CancellationTokenSource();
        }

        protected virtual void OnDisable()
        {
            CancelInvoke();
            cts?.Cancel();
            StackObjectPool.ReturnToPool(gameObject);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(checkRangePoint, atkRange);
        }

        public void UnitSetup(int unitDamage, float atkDelay, float attackRange = 2)
        {
            damage = unitDamage;
            _atkDelay = atkDelay;
            atkRange = attackRange;
        }

        protected abstract void Attack();

        protected async UniTaskVoid StartCoolDown()
        {
            attackAble = false;
            await UniTask.Delay(TimeSpan.FromSeconds(_atkDelay), cancellationToken: cts.Token);
            attackAble = true;
        }
    }
}