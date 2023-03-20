using System.Threading;
using AttackControl;
using UnityEngine;

namespace UnitControl
{
    public abstract class Unit : MonoBehaviour
    {
        private CancellationTokenSource _cts;

        protected TargetFinder targetFinder;

        protected abstract void CheckState();
        protected abstract void Attack();

        protected virtual void Awake()
        {
            targetFinder = GetComponent<TargetFinder>();
        }

        protected virtual void OnEnable()
        {
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
            InvokeRepeating(nameof(CheckState), 0, 0.5f);
        }

        protected virtual void OnDisable()
        {
            _cts?.Cancel();
            CancelInvoke();
        }
    }
}