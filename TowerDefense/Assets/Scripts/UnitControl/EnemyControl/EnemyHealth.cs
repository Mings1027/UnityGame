using System;
using StatusControl;

namespace UnitControl.EnemyControl
{
    public class EnemyHealth : Health
    {
        public event Action OnUpdateEnemyCountEvent;

        protected override void OnDisable()
        {
            base.OnDisable();

            OnUpdateEnemyCountEvent?.Invoke();
            OnUpdateEnemyCountEvent = null;
        }
    }
}