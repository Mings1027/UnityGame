using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using PoolObjectControl;
using TowerControl;
using UnityEngine;

namespace ManagerControl
{
    public class TowerManager : MonoBehaviour
    {
        private CancellationTokenSource _cts;
        private List<Tower> _towers;
        private UnitTower _unitTower;

        #region Unity Event

        protected void Awake()
        {
            _towers = new List<Tower>(50);
        }

        private void OnDisable()
        {
            if (_cts.IsCancellationRequested) return;
            _cts?.Cancel();
            _cts?.Dispose();
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            Application.targetFrameRate = pauseStatus ? 20 : 60;
        }

        #endregion

        #region TowerControl

        public void StartTargeting()
        {
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
            Application.targetFrameRate = 60;
            TowerTargeting().Forget();
            TowerUpdate().Forget();
        }

        public void StopTargeting()
        {
            if (!_cts.IsCancellationRequested)
            {
                _cts?.Cancel();
                _cts?.Dispose();
            }

            TargetInit();
        }

        private async UniTaskVoid TowerTargeting()
        {
            while (!_cts.IsCancellationRequested)
            {
                await UniTask.Delay(100, cancellationToken: _cts.Token);
                if (Time.timeScale == 0) continue;

                var towerCount = _towers.Count;
                for (var i = 0; i < towerCount; i++)
                {
                    _towers[i].TowerTargeting();
                }
            }
        }

        private async UniTaskVoid TowerUpdate()
        {
            while (!_cts.IsCancellationRequested)
            {
                await UniTask.Delay(10, cancellationToken: _cts.Token);
                if (Time.timeScale == 0) continue;

                var towerCount = _towers.Count;
                for (var i = 0; i < towerCount; i++)
                {
                    _towers[i].TowerUpdate(_cts);
                }
            }
        }

        private void TargetInit()
        {
            var towerCount = _towers.Count;
            for (var i = 0; i < towerCount; i++)
            {
                _towers[i].TowerTargetInit();
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