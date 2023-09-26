using System;
using ManagerControl;
using StatusControl;

namespace UnitControl.EnemyControl
{
    public class EnemyHealth : Health
    {
        public event Action OnUpdateEnemyCountEvent;

        protected override void OnDisable()
        {
            base.OnDisable();
            if (CurrentProgress > 0)
            {
                TowerManager.Instance.DecreaseLifeCountEvent();
            }

            OnUpdateEnemyCountEvent?.Invoke();
            OnUpdateEnemyCountEvent = null;
        }
    }
}   