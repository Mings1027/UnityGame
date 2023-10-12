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

            OnUpdateEnemyCountEvent = null;
        }

        public void DecreaseEnemyCount()
        {
            OnUpdateEnemyCountEvent?.Invoke();
            OnUpdateEnemyCountEvent = null;
        }

        public override void Damage(in float amount)
        {
            base.Damage(in amount);
            if (Current > 0) return;
            DecreaseEnemyCount();
        }
    }
}