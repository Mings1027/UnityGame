using System;
using System.Collections.Generic;
using System.Threading;
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

        public async UniTaskVoid TowerUpgrade(Tower selectedTower, TowerLevelManager towerLevelManager)
        {
            StackObjectPool.Get("BuildSmoke", selectedTower.transform.position + new Vector3(0, 7, 0));
            var towerIndexLevel = towerLevelManager.towerLevels[selectedTower.towerLevel];

            selectedTower.Init(towerIndexLevel.health,
                towerIndexLevel.Damage,
                towerIndexLevel.attackRange,
                towerIndexLevel.attackDelay);
            selectedTower.ChangeMesh(towerIndexLevel.consMesh);

            await UniTask.Delay(TimeSpan.FromSeconds(towerIndexLevel.constructionTime), cancellationToken: _cts.Token);

            selectedTower.ChangeMesh(towerIndexLevel.towerMesh);

            selectedTower.SetUp();
        }
    }
}