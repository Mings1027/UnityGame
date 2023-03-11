using System.Threading;
using AttackControl;
using GameControl;
using UnityEngine;

namespace UnitControl
{
    public abstract class Unit : MonoBehaviour
    {
        private CancellationTokenSource _cts;

        protected TargetFinder targetFinder;
        protected bool isTargeting;
        public Transform target;


        protected virtual void Awake()
        {
            targetFinder = GetComponent<TargetFinder>();
        }

        protected virtual void OnEnable()
        {
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
            InvokeRepeating(nameof(FoundTarget), 0, 1f);
        }

        protected virtual void OnDisable()
        {
            CancelInvoke();
            _cts?.Cancel();
            StackObjectPool.ReturnToPool(gameObject);
        }

        protected virtual void Update()
        {
            if (!isTargeting || !targetFinder.attackAble) return;
            Attack();
            targetFinder.StartCoolDown().Forget();
        }

        protected abstract void Attack();

        private void FoundTarget()
        {
            var t = targetFinder.FindClosestTarget();
            target = t.Item1;
            isTargeting = t.Item2;
        }
    }
}