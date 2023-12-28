using System.Collections.Generic;
using TowerControl;
using UnityEngine;

namespace ManagerControl
{
    public class TowerManager : MonoBehaviour
    {
        private List<AttackTower> _towers;
        [SerializeField] private TowerMana towerMana;

        #region Unity Event

        protected void Awake()
        {
            _towers = new List<AttackTower>(50);
        }

        private void Start()
        {
            Application.targetFrameRate = 60;
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
            towerMana.towerMana.StartManaRegen();
            ReadyToTarget();
        }

        public void StopTargeting()
        {
            enabled = false;
            TargetInit();
            towerMana.towerMana.StopManaRegen();
        }

        private void ReadyToTarget()
        {
            var towerCount = _towers.Count;
            for (int i = 0; i < towerCount; i++)
            {
                if (_towers[i].TryGetComponent(out UnitTower unitTower))
                {
                    unitTower.ActiveAnim();
                }
            }
        }
        private void TargetInit()
        {
            var towerCount = _towers.Count;
            for (var i = 0; i < towerCount; i++)
            {
                _towers[i].TowerTargetInit();
                if (_towers[i].TryGetComponent(out UnitTower unitTower))
                {
                    unitTower.DeActiveAnim();
                }
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