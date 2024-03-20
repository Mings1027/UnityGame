using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using InterfaceControl;
using TowerControl;
using UIControl;
using UnityEngine;

namespace ManagerControl
{
    public class TowerManager : MonoBehaviour, IMainGameObject
    {
        private CancellationTokenSource _cts;
        private List<AttackTower> _towers;

#region Unity Event

        private void OnDisable()
        {
            if (_cts == null || _cts.IsCancellationRequested) return;
            _cts?.Cancel();
            _cts?.Dispose();
        }

#endregion

        public void Init()
        {
            _towers = new List<AttackTower>(50);
        }

#region TowerControl

        public void StartTargeting()
        {
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
            GameHUD.towerMana.StartManaRegen();
            TowerUpdate().Forget();
        }

        public void StopTargeting()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            TargetInit();
            GameHUD.towerMana.StopManaRegen();
        }

        private void TargetInit()
        {
            var towerCount = _towers.Count;

            for (var i = 0; i < towerCount; i++)
            {
                _towers[i].TowerTargetInit();
            }
        }

        private async UniTaskVoid TowerUpdate()
        {
            while (!_cts.IsCancellationRequested)
            {
                await UniTask.Yield(cancellationToken: _cts.Token);
                var towerCount = _towers.Count;
                for (int i = 0; i < towerCount; i++)
                {
                    _towers[i].TowerUpdate();
                }
            }
        }

#endregion

#region Public Method

        public void AddTower(AttackTower tower)
        {
            _towers.Add(tower);
        }

        public void RemoveTower(AttackTower tower)
        {
            _towers.Remove(tower);
        }

#endregion
    }
}