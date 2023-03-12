using Cysharp.Threading.Tasks;
using GameControl;
using TowerControl;
using UnityEngine;
//
// namespace BuildControl
// {
//     public abstract class TowerLeveling 
//     {
//         public static async UniTaskVoid TowerUpgrade(int uniqueLevel, TowerBase selectedTower,
//             TowerLevelManager towerLevelManager)
//         {
//             selectedTower.TowerLevelUp(uniqueLevel);
//             StackObjectPool.Get("BuildSmoke", selectedTower.transform.position + new Vector3(0, 7, 0));
//             var t = towerLevelManager.towerLevels[selectedTower.TowerLevel];
//
//             selectedTower.ReadyToBuild(t.consMesh);
//             
//             await UniTask.Delay(1000);
//             
//             selectedTower.Building(t.attackDelay, t.attackRange, t.Damage, t.health, t.towerMesh);
//         }
//     }
// }