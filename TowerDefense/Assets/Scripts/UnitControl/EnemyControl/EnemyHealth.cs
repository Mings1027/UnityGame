using System;
using InterfaceControl;
using ManagerControl;
using StatusControl;
using UnityEngine;

namespace UnitControl.EnemyControl
{
    public class EnemyHealth : Health
    {
        public event Action OnUpdateEnemyCountEvent;

        private void OnDisable()
        {
            if (CurrentProgress > 0)
            {
                TowerManager.Instance.DecreaseLifeCountEvent();
            }

            OnUpdateEnemyCountEvent?.Invoke();
            OnUpdateEnemyCountEvent = null;
        }
    }
}