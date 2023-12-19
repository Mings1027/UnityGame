using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using TowerControl;
using UIControl;
using UnityEngine;

namespace ManagerControl
{
    public class TowerManager : MonoBehaviour
    {
        private List<Tower> _towers;
        private UnitTower _unitTower;
        private CancellationTokenSource _cts;

        #region Unity Event

        protected void Awake()
        {
            _towers = new List<Tower>(50);
        }

        private void Start()
        {
            Application.targetFrameRate = 60;
        }

        // private void Update()
        // {
        //     var towerCount = _towers.Count;
        //     for (var i = 0; i < towerCount; i++)
        //     {
        //         _towers[i].TowerUpdate();
        //     }
        // }
        private void OnDisable()
        {
            if (_cts == null) return;
            if (_cts.IsCancellationRequested) return;
            _cts?.Cancel();
            _cts?.Dispose();
        }

        #endregion

        #region TowerControl

        public void StartTargeting()
        {
            Application.targetFrameRate = 60;
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
            UIManager.Instance.Mana.StartManaRegen();
            TowerUpdate().Forget();
        }

        public void StopTargeting()
        {
            if (_cts.IsCancellationRequested) return;
            _cts?.Cancel();
            _cts?.Dispose();
            TargetInit();
            UIManager.Instance.Mana.StopManaRegen();
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

        private async UniTaskVoid TowerUpdate()
        {
            while (!_cts.IsCancellationRequested)
            {
                await UniTask.Delay(10);
                var towerCount = _towers.Count;
                for (var i = 0; i < towerCount; i++)
                {
                    _towers[i].TowerUpdate();
                }
            }
        }

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