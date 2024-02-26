using System;
using System.Collections.Generic;
using DataControl;
using DataControl.TowerDataControl;
using InterfaceControl;
using StatusControl;
using TowerControl;
using UnityEngine;

namespace ManagerControl
{

    public class TowerManager : MonoBehaviour, IAddressableObject
    {
        private List<AttackTower> _towers;
        private Mana _towerMana;

#region Unity Event

        private void Start()
        {
            _towers = new List<AttackTower>(50);
        }

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
            enabled = false;
        }

#region TowerControl

        public void StartTargeting()
        {
            enabled = true;
            _towerMana.StartManaRegen();
        }

        public void StopTargeting()
        {
            enabled = false;
            TargetInit();
            _towerMana.StopManaRegen();
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

        public void TowerManaInit(Mana towerMana)
        {
            _towerMana = towerMana;
        }

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