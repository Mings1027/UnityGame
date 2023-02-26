using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameControl;
using UnityEngine;

namespace UnitControl
{
    public class BarracksUnit : Unit
    {
        private float _atkDelay;
        private Transform _target;

        public event Action OnDeadEvent;

        protected override void OnEnable()
        {
            base.OnEnable();
        }

        private void Update()
        {
            if (attackable)
            {
                AttackTask().Forget();
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            OnDeadEvent?.Invoke();
            OnDeadEvent = null;
        }

        private async UniTaskVoid AttackTask()
        {
            attackable = false;
            Attack();
            await UniTask.Delay(TimeSpan.FromSeconds(_atkDelay), cancellationToken: Cts.Token);
            attackable = true;
        }

        private void Attack()
        {
            print("atttttttttttttttaaackkkkkk");
        }


        public void UnitSetUp(Transform target, float atkDelay)
        {
            _target = target;
            _atkDelay = atkDelay;
        }
    }
}