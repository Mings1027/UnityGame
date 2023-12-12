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

        // private void Update()
        // {
        //     var towerCount = _towers.Count;
        //     for (var i = 0; i < towerCount; i++)
        //     {
        //         _towers[i].TowerUpdate();
        //     }
        // }

        private void OnApplicationPause(bool pauseStatus)
        {
            Application.targetFrameRate = pauseStatus ? 20 : 60;
        }

        #endregion

        #region TowerControl

        public void StartTargeting()
        {
            enabled = true;
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
            Application.targetFrameRate = 60;
            UIManager.Instance.Mana.StartManaRegen();
            TowerUpdate().Forget();
        }

        public void StopTargeting()
        {
            enabled = false;
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