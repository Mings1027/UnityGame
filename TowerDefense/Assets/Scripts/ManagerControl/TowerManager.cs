using System.Collections.Generic;
using TowerControl;
using UIControl;
using UnityEngine;

namespace ManagerControl
{
    public class TowerManager : MonoBehaviour
    {
        private List<Tower> _towers;
        private UnitTower _unitTower;

        #region Unity Event

        protected void Awake()
        {
            _towers = new List<Tower>(50);
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
            UIManager.Instance.Mana.StartManaRegen();
            // TowerUpdate().Forget();
        }

        public void StopTargeting()
        {
            enabled = false;
            TargetInit();
            UIManager.Instance.Mana.StopManaRegen();
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

        public void AddTower(Tower tower)
        {
            _towers.Add(tower);
        }

        public void RemoveTower(Tower tower)
        {
            _towers.Remove(tower);
        }

        #endregion
    }
}