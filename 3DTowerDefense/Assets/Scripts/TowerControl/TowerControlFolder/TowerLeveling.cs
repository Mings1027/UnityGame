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

        public bool typeA;

        private void Awake()
        {
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
        }

        private void OnDestroy()
        {
            _cts.Cancel();
        }

        public void TowerUpgrade(Tower selectedTower, TowerLevelManager towerLevelManager)
        {
            Upgrade(selectedTower, towerLevelManager).Forget();
        }

        private async UniTaskVoid Upgrade(Tower selectedTower, TowerLevelManager towerLevelManager)
        {
            if (selectedTower.towerLevel < 2) selectedTower.towerLevel++;
            else
            {
                selectedTower.towerLevel = typeA ? 3 : 4;
            }
            selectedTower.isUpgrading = true;
            StackObjectPool.Get("BuildSmoke", selectedTower.transform.position + new Vector3(0, 7, 0));
            var towerIndexLevel = towerLevelManager.towerLevels[selectedTower.towerLevel];

            selectedTower.meshFilter.sharedMesh = towerIndexLevel.consMesh.sharedMesh;

            await UniTask.Delay(TimeSpan.FromSeconds(towerIndexLevel.constructionTime), cancellationToken: _cts.Token);

            selectedTower.meshFilter.sharedMesh = towerIndexLevel.towerMesh.sharedMesh;

            selectedTower.atkRange = towerIndexLevel.attackRange;
            selectedTower.atkDelay = towerIndexLevel.attackDelay;
            selectedTower.isUpgrading = false;
            typeA = false;
            selectedTower.Init();
        }
    }
}