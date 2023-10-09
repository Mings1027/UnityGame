using System;
using DG.Tweening;
using InterfaceControl;
using PoolObjectControl;
using UnityEngine;

namespace StatusControl
{
    public class Health : Progressive, IDamageable, IHealable
    {
        private Collider _collider;

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
            CurrentProgress -= amount;

            PoolObjectManager.Get(PoolObjectKey.BloodVfx, transform.position);
            if (CurrentProgress > 0f) return;
            _collider.enabled = false;
            OnDeadEvent?.Invoke();
        }

        public void Heal(in float amount)
        {
            CurrentProgress += amount;

            if (CurrentProgress > Initial)
            {
                CurrentProgress = Initial;
            }
        }
    }
}