using System;
using InterfaceControl;

namespace StatusControl
{
    public abstract class Health : Progressive, IDamageable, IHealable
    {
        public bool IsDead => Current <= 0;
        public event Action OnDeadEvent;

        protected override void OnDisable()
        {
            OnDeadEvent = null;
        }

        public virtual void Damage(float amount)
        {
            Current -= amount;

            if (Current > 0f) return;
            OnDeadEvent?.Invoke();
        }

        public void Heal(in float amount)
        {
            Current += amount;

            if (Current > Initial)
            {
                Current = Initial;
            }
        }
    }
}