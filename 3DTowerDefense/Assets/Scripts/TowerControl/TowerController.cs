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
        public static void MoveBarrackUnit(InputManager input, Camera cam, Tower curTower,
            MeshRenderer moveUnitIndicator)
        {
            var ray = cam.ScreenPointToRay(input.MousePos);
            if (!Physics.Raycast(ray, out var hit)) return;
            if (Vector3.Distance(curTower.transform.position, hit.point) < curTower.TowerRange)
            {
                if (!hit.collider.CompareTag("Ground")) return;
                input.IsMoveUnit = false;
                moveUnitIndicator.enabled = false;
                curTower.GetComponent<BarracksTower>().MoveUnits(hit.point);
            }
            else
            {
                print("Can't Move");
            }
        }

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