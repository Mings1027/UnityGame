using System.Collections.Generic;
using InterfaceControl;
using TowerControl;
using UIControl;
using UnityEngine;

namespace ManagerControl
{
    public class TowerManager : MonoBehaviour, IMainGameObject
    {
        private List<AttackTower> _towers;

#region Unity Event

        private void Update()
        {
            var towerCount = _towers.Count;

            for (var i = 0; i < towerCount; i++)
            {
                _towers[i].TowerUpdate();
            }
        }

#endregion

        public void Init()
        {
            _towers = new List<AttackTower>(50);
            enabled = false;
        }

#region TowerControl

        public void StartTargeting()
        {
            enabled = true;
            GameHUD.towerMana.StartManaRegen();
        }

        public void StopTargeting()
        {
            enabled = false;
            TargetInit();
            GameHUD.towerMana.StopManaRegen();
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

#region Public Method

        public void AddTower(AttackTower tower)
        {
            _towers.Add(tower);
        }

        public void RemoveTower(AttackTower tower)
        {
            _towers.Remove(tower);
        }

#endregion
    }
}