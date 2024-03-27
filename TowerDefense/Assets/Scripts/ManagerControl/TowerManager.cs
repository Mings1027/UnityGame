using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using InterfaceControl;
using TowerControl;
using UnityEngine;

namespace ManagerControl
{
    public class TowerManager : MonoBehaviour, IMainGameObject
    {
        private CancellationTokenSource _cts;
        private List<Tower> _towers;

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
            _towers = new List<Tower>(50);
        }

#region TowerControl

        public void StartTargeting()
        {
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
            TowerUpdate().Forget();
        }

        public void StopTargeting()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            TargetInit();
        }

        private void TargetInit()
        {
            var towerCount = _towers.Count;

            for (var i = 0; i < towerCount; i++)
            {
                _towers[i].TowerPause();
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

        public void AddTower(Tower tower)
        {
            _towers.Add(tower);
        }

        public void RemoveTower(Tower tower)
        {
            _towers.Remove(tower);
        }

#endregion
    }
}