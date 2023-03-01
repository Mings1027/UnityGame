using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using GameControl;
using UnitControl;
using UnityEngine;

namespace TowerControl
{
    public class BarracksTower : Tower
    {
        private readonly BarracksUnit[] _units = new BarracksUnit[3];
        private int _deadCount;

        protected override void OnEnable()
        {
            base.OnEnable();
            SpawnUnit().Forget();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            foreach (var t in _units)
            {
                if (t != null && t.gameObject.activeSelf) t.gameObject.SetActive(false);
            }
        }

        protected override void Targeting()
        {
        }


        private async UniTaskVoid SpawnUnit()
        {
            await UniTask.Delay(millisecondsDelay: 3000, cancellationToken: cts.Token);
            _deadCount = 3;
            var unitName = towerLevel == 4 ? "SpearManUnit" : "SwordManUnit";

            for (var i = 0; i < _deadCount; i++)
            {
                _units[i] = StackObjectPool.Get<BarracksUnit>(unitName, transform.position);
                _units[i].OnDeadEvent += DeadCount;
            }
        }

        private void DeadCount()
        {
            _deadCount -= 1;
            if (_deadCount <= 0) SpawnUnit().Forget();
        }
    }
}