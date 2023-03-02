using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameControl;
using UnityEngine;

namespace UnitControl
{
    public abstract class Unit : MonoBehaviour
    {
        private int _health;
        private float _atkDelay;
        private Transform _target;

        protected bool attackAble;
        protected CancellationTokenSource cts;
        
        public bool IsTargeting { get; set; }

        public Transform Target
        {
            get => _target;
            set => _target = IsTargeting ? value : null;
        }


        private void OnEnable()
        {
            attackAble = true;
            cts?.Dispose();
            cts = new CancellationTokenSource();
        }

        protected virtual void OnDisable()
        {
            cts?.Cancel();
            StackObjectPool.ReturnToPool(gameObject);
        }


        public void Init(float atkDelay)
        {
            _atkDelay = atkDelay;
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