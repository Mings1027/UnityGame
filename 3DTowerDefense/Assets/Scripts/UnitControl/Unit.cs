using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameControl;
using UnityEngine;

namespace UnitControl
{
    public abstract class Unit : MonoBehaviour
    {
        private float _atkDelay;
        private bool _attackAble;
        private Transform _targetPos;

        public bool IsTargeting { get; set; }

        public Transform TargetPos
        {
            get => _targetPos;
            set => _targetPos = IsTargeting ? value : null;
        }

        protected CancellationTokenSource cts;

        private void OnEnable()
        {
            _attackAble = true;
            cts?.Dispose();
            cts = new CancellationTokenSource();
        }

        protected virtual void OnDisable()
        {
            cts?.Cancel();
            StackObjectPool.ReturnToPool(gameObject);
        }

        private void FixedUpdate()
        {
            if (!_attackAble || !IsTargeting) return;
            Attack();
            StartCoolDown().Forget();
        }

        public void Init(float atkDelay)
        {
            _atkDelay = atkDelay;
        }

        protected abstract void Attack();

        private async UniTaskVoid StartCoolDown()
        {
            _attackAble = false;
            await UniTask.Delay(TimeSpan.FromSeconds(_atkDelay), cancellationToken: cts.Token);
            _attackAble = true;
        }
    }
}