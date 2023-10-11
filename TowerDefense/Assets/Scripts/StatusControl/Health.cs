using System;
using CustomEnumControl;
using InterfaceControl;
using PoolObjectControl;
using UnityEngine;

namespace StatusControl
{
    public class Health : Progressive, IDamageable, IHealable
    {
        private Collider _collider;

        public bool IsDead => Current <= 0;
        public event Action OnDeadEvent;

        private void Awake()
        {
            _collider = GetComponent<Collider>();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            _collider.enabled = true;
        }

        protected virtual void OnDisable()
        {
            OnDeadEvent = null;
        }

        public void Damage(in float amount)
        {
            if (IsDead) return;
            Current -= amount;

            PoolObjectManager.Get(PoolObjectKey.BloodVfx, transform.position);
            if (Current > 0f) return;
            _collider.enabled = false;
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