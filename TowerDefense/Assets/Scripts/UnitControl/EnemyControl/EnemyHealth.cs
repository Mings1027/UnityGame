using System;
using StatusControl;

namespace UnitControl.EnemyControl
{
    public class EnemyHealth : Health
    {
        public event Action OnDecreaseLifeCountEvent;
        public event Action OnUpdateEnemyCountEvent;

        protected override void OnDisable()
        {
            base.OnDisable();
            if (Current > 0)
            {
                OnDecreaseLifeCountEvent?.Invoke();
            }

            OnUpdateEnemyCountEvent?.Invoke();
            OnUpdateEnemyCountEvent = null;
            OnDecreaseLifeCountEvent = null;
        }
    }
}