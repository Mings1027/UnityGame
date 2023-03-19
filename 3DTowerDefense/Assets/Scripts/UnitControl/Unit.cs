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
        protected Transform target;

        protected bool IsTargeting { get; private set; }

        protected virtual void Awake()
        {
            targetFinder = GetComponent<TargetFinder>();
            targetFinder.colliderSize = 3;
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
        }

        private void FoundTarget()
        {
            var t = targetFinder.FindClosestTarget();
            target = t.Item1;
            IsTargeting = t.Item2;
        }
    }
}