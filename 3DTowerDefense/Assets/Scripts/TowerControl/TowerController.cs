using System.Collections.Generic;
using BuildControl;
using Cysharp.Threading.Tasks;
using GameControl;
using ManagerControl;
using UnityEngine;

namespace TowerControl
{
    public class TowerController : MonoBehaviour
    {
        public static async UniTaskVoid TowerUpgrade(int level, IReadOnlyList<TowerLevelManager> towerLevelManagers,
            Tower selectedTower)
        {
            selectedTower.TowerLevelUp(level);
            StackObjectPool.Get("BuildSmoke", selectedTower.transform.position);
            var t = towerLevelManagers[(int)selectedTower.TowerType];
            var tt = t.towerLevels[selectedTower.TowerLevel];
            selectedTower.UnderConstruction(tt.consMesh);

            if (selectedTower.TowerType == Tower.Type.Barracks)
            {
                selectedTower.GetComponent<BarracksTower>().UnitHealth = tt.health;
            }

            await UniTask.Delay(1000);

            selectedTower.ConstructionFinished(tt.towerMesh, tt.minDamage, tt.maxDamage, tt.attackRange,
                tt.attackDelay);
        }
    }
}