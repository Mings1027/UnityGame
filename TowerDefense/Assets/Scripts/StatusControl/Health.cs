using System;
using CustomEnumControl;
using InterfaceControl;
using PoolObjectControl;
using TextControl;

namespace StatusControl
{
    public abstract class Health : Progressive, IDamageable, IHealable
    {
        public bool IsDead => Current <= 0;
        public event Action OnDeadEvent;
        public event Action OnShakeEvent;

        protected override void OnDisable()
        {
            OnDeadEvent = null;
        }

        public virtual void Damage(float amount)
        {
            Current -= amount;
            OnShakeEvent?.Invoke();
            if (Current > 0f) return;
            OnDeadEvent?.Invoke();
        }

        public void Heal(in float amount)
        {
            if (IsDead) return;
            if (Current >= Initial) return;

            Current += amount;
            PoolObjectManager.Get<FloatingText>(UIPoolObjectKey.FloatingText, transform.position)
                .SetHpText((ushort)amount);

            if (Current > Initial)
            {
                Current = Initial;
            }
        }
    }
}