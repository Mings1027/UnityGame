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
        private GameObject _unit1, _unit2, _unit3;
        private int _deadCount;

        private async UniTaskVoid SpawnUnit()
        {
            await UniTask.Delay(3000, cancellationToken: Cts.Token);
            _deadCount = 3;
            var unitName = towerLevel == 4 ? "SpearManUnit" : "SwordManUnit";

            for (var i = 0; i < _deadCount; i++)
            {
                StackObjectPool.Get<BarracksUnit>(unitName, transform.position).OnDeadEvent += DeadCount;
            }
        }

        private void DeadCount()
        {
            _deadCount -= 1;
            if (_deadCount <= 0) SpawnUnit().Forget();
        }
    }
}