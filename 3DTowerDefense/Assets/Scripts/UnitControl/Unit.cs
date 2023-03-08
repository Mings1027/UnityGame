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

        protected bool isTargeting;
        protected bool attackAble;
        
        public Transform target;
        public int damage;

        protected virtual void OnEnable()
        {
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
        }

        protected virtual void OnDisable()
        {
            CancelInvoke();
            _cts?.Cancel();
            StackObjectPool.ReturnToPool(gameObject);
        }

        public abstract void Attack();

        protected async UniTaskVoid StartCoolDown()
        {
            attackAble = false;
            await UniTask.Delay(1000, cancellationToken: _cts.Token);
            attackAble = true;
        }

        public void UnitSetUp(int amount)
        {
            damage = amount;
        }
    }
}