using System;
using System.Collections.Generic;
using System.Threading;
using AttackControl;
using Cysharp.Threading.Tasks;
using GameControl;
using UnityEngine;

namespace TowerControl.TowerControlFolder
{
    public class TowerLeveling : MonoBehaviour
    {
        private CancellationTokenSource _cts;

        private void Awake()
        {
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
        }

        private void OnDestroy()
        {
            _cts.Cancel();
        }

        public async UniTaskVoid TowerUpgrade(int uniqueLevel, Tower selectedTower, TowerLevelManager towerLevelManager)
        {
            if (selectedTower.towerLevel < 2) selectedTower.towerLevel++;
            else if (uniqueLevel > 0) selectedTower.towerLevel = uniqueLevel;
            StackObjectPool.Get("BuildSmoke", selectedTower.transform.position + new Vector3(0, 7, 0));
            var towerIndexLevel = towerLevelManager.towerLevels[selectedTower.towerLevel];

            selectedTower.UnitInit();
            selectedTower.GetComponent<TargetFinder>().RangeSetUp(towerIndexLevel.attackRange);

            selectedTower.ChangeMesh(towerIndexLevel.consMesh);

            await UniTask.Delay(TimeSpan.FromSeconds(towerIndexLevel.constructionTime), cancellationToken: _cts.Token);

            selectedTower.ChangeMesh(towerIndexLevel.towerMesh);

            selectedTower.SetUp(towerIndexLevel.attackDelay,
                towerIndexLevel.Damage, towerIndexLevel.health);
        }
    }
}