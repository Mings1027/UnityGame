using System;
using CustomEnumControl;
using InterfaceControl;
using PoolObjectControl;
using TextControl;
// using TextControl;
using Random = UnityEngine.Random;

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

        public virtual void Damage(int amount)
        {
            Current -= amount;
            OnShakeEvent?.Invoke();

            if (Current > 0f) return;
            OnDeadEvent?.Invoke();
        }

        public virtual void Heal(in ushort amount)
        {
            if (IsDead) return;
            if (Current >= Initial) return;
            Current += amount;
            PoolObjectManager.Get<FloatingText>(UIPoolObjectKey.HealText,
                transform.position + Random.insideUnitSphere).SetHpText(amount);
        }
    }
}