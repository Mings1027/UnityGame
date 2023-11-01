using System;
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

        [Header("----------Indicator----------"), SerializeField]
        private MeshRenderer rangeIndicator;

        [SerializeField] private MeshRenderer selectedTowerIndicator;
        [SerializeField] private MeshRenderer unitDestinationIndicator;

        #region Unity Event

        private void Awake()
        {
            _towers = new List<Tower>(50);
        }

        private void Start()
        {
            IndicatorInit();
        }

        #endregion

        #region TowerControl

        public void StartTargeting()
        {
            _cts?.Dispose();
            _cts = new CancellationTokenSource();

            TargetingAsync().Forget();
            AttackAsync().Forget();
        }

        public void StopTargeting()
        {
            _cts?.Cancel();

            TargetInit();
            PoolObjectManager.PoolCleaner().Forget();
        }

        private async UniTaskVoid TargetingAsync()
        {
            while (!_cts.IsCancellationRequested)
            {
                await UniTask.Delay(500, cancellationToken: _cts.Token);
                for (var i = 0; i < _towers.Count; i++)
                {
                    _towers[i].TowerTargeting();
                    await UniTask.Delay(100, cancellationToken: _cts.Token);
                }
            }
        }

        private async UniTaskVoid AttackAsync()
        {
            while (!_cts.IsCancellationRequested)
            {
                await UniTask.Delay(100, cancellationToken: _cts.Token);
                for (var i = 0; i < _towers.Count; i++)
                {
                    _towers[i].TowerAttackAsync(_cts);
                    await UniTask.Delay(10, cancellationToken: _cts.Token);
                }
            }
        }

        private void TargetInit()
        {
            for (var i = 0; i < _towers.Count; i++)
            {
                _towers[i].TowerTargetInit();
            }
        }

        #endregion

        private void IndicatorInit()
        {
            rangeIndicator.transform.localScale = Vector3.zero;
            selectedTowerIndicator.enabled = false;
            unitDestinationIndicator.enabled = false;
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

        public void SetIndicator(Tower curSelectedTower)
        {
            var curTowerPos = curSelectedTower.transform.position;
            selectedTowerIndicator.transform.position = curTowerPos;
            selectedTowerIndicator.enabled = true;

            var r = rangeIndicator.transform;
            r.DOScale(new Vector3(curSelectedTower.TowerRange, 0.5f, curSelectedTower.TowerRange), 0.15f)
                .SetEase(Ease.OutBack);
            r.position = curTowerPos;
        }

        public void OffIndicator()
        {
            rangeIndicator.transform.DOScale(0, 0.2f).SetEase(Ease.InBack);

            if (selectedTowerIndicator.enabled)
            {
                selectedTowerIndicator.enabled = false;
            }
        }

        public async UniTaskVoid StartMoveUnit(UnitTower unitTower, Vector3 pos)
        {
            unitDestinationIndicator.enabled = true;
            unitDestinationIndicator.transform.position = pos;

            await unitTower.StartUnitMove(pos);
            unitDestinationIndicator.enabled = false;
        }

        #endregion
    }
}