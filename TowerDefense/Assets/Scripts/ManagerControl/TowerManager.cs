using System;
using System.Collections.Generic;
using DataControl;
using DataControl.TowerDataControl;
using StatusControl;
using TowerControl;
using UnityEngine;

namespace ManagerControl
{
    [Serializable]
    public class TowerDataPrefab
    {
        public TowerData towerData;
        public GameObject towerPrefab;
    }

    public class TowerManager : MonoBehaviour
    {
        private List<AttackTower> _towers;
        private Mana _towerMana;

        [field: SerializeField] public TowerDataPrefab[] towerDataPrefabs{ get; private set; }

#region Unity Event

        private void Start()
        {
            enabled = false;
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

#region TowerControl

        public void StartTargeting()
        {
            enabled = true;
            Application.targetFrameRate = 60;
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

        public void AddTower(AttackTower tower)
        {
            _towers.Add(tower);
        }

        public void RemoveTower(AttackTower tower)
        {
            _towers.Remove(tower);
        }

        public void Init()
        {
            _towers = new List<AttackTower>(50);
            _towerMana = UIManager.Instance.GetTowerMana();
        }

#endregion
    }
}