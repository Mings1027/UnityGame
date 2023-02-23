using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameControl;
using UnityEngine;

namespace TowerControl.TowerControlFolder
{
    public class TowerLeveling : MonoBehaviour
    {
        private CancellationTokenSource _cts;

        public bool isUnique;
        public bool typeA;
        public bool isUpgrade;

        private void Awake()
        {
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
        }

        private void OnDestroy()
        {
            _cts.Cancel();
        }

        public async UniTask TowerUpgrade(Tower selectedTower, TowerLevelManager[] towerLevelManager)
        {
            isUpgrade = true;
            selectedTower.towerLevel++;
            var index = 0;
            if (isUnique)
            {
                index = typeA ? 3 : 4;
            }

            StackObjectPool.Get("BuildSmoke", selectedTower.transform.position + new Vector3(0, 6, 0));
            var towerIndexLevel = towerLevelManager[(int)selectedTower.towerType]
                .towerLevels[isUnique ? index : selectedTower.towerLevel];

            selectedTower.meshFilter.sharedMesh = towerIndexLevel.consMesh.sharedMesh;

            await UniTask.Delay(TimeSpan.FromSeconds(towerIndexLevel.constructionTime), cancellationToken: _cts.Token);

            selectedTower.meshFilter.sharedMesh = towerIndexLevel.towerMesh.sharedMesh;

            selectedTower.atkRange = towerIndexLevel.attackRange;
            selectedTower.atkDelay = towerIndexLevel.attackDelay;
            isUpgrade = false;
            isUnique = false;
            typeA = false;
            selectedTower.Init();
        }
    }
}