using System;
using StatusControl;

namespace UnitControl.EnemyControl
{
    public class EnemyHealth : Health
    {
        public event Action OnDecreaseEnemyCountEvent;

        public override void Damage(in float amount)
        {
            base.Damage(in amount);
            if (Current > 0) return;
            DecreaseEnemyCount();
        }

        public void DecreaseEnemyCount()
        {
            OnDecreaseEnemyCountEvent?.Invoke();
            OnDecreaseEnemyCountEvent = null;
        }
    }
}