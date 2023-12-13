using System;
using CustomEnumControl;
using InterfaceControl;
using PoolObjectControl;
using TextControl;
using UnityEngine;
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

        public virtual void Damage(float amount)
        {
            Current -= amount;
            OnShakeEvent?.Invoke();

            PoolObjectManager.Get<FloatingText>(UIPoolObjectKey.DamageText,
                    transform.position + Random.insideUnitSphere)
                .SetHpText((ushort)amount, false);
            if (Current > 0f) return;
            OnDeadEvent?.Invoke();
        }

        public virtual void Heal(in float amount)
        {
            if (IsDead) return;
            if (Current >= Initial) return;

            Current += amount;
            PoolObjectManager.Get<FloatingText>(UIPoolObjectKey.HealText,
                    transform.position + Random.insideUnitSphere)
                .SetHpText((ushort)amount);

            if (Current > Initial)
            {
                Current = Initial;
            }
        }
    }
}