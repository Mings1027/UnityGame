using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameControl;
using UnityEngine;

namespace UnitControl
{
    public abstract class Unit : MonoBehaviour
    {
        private CancellationTokenSource _cts;
        private float _atkDelay;
        private bool _attackAble;
        private bool _isTargeting;

        protected Vector3 targetPos;

        private void OnEnable()
        {
            _attackAble = true;
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
        }

        protected virtual void OnDisable()
        {
            _cts?.Cancel();
            StackObjectPool.ReturnToPool(gameObject);
        }

        private void FixedUpdate()
        {
            if (!_attackAble || !_isTargeting) return;
            Attack();
            StartCoolDown().Forget();
        }

        public void Init(float atkDelay)
        {
            _atkDelay = atkDelay;
        }

        public void UpdateTarget(bool isTargeting, Vector3 t)
        {
            _isTargeting = isTargeting;
            targetPos = t;
        }

        protected abstract void Attack();

        private async UniTaskVoid StartCoolDown()
        {
            _attackAble = false;
            await UniTask.Delay(TimeSpan.FromSeconds(_atkDelay), cancellationToken: _cts.Token);
            _attackAble = true;
        }
    }
}