using System;
using InterfaceControl;
using StatusControl;
using UnityEngine;

namespace UnitControl.EnemyControl
{
    public class EnemyHealth : Progressive, IDamageable, IHealable
    {
        private Collider _collider;

        public event Action OnUpdateEnemyCountEvent;
        public event Action OnDecreaseLifeCountEvent;
        public event Action OnDieEvent;

        protected override void Awake()
        {
            base.Awake();
            _collider = GetComponent<Collider>();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            _collider.enabled = true;
        }

        private void OnDisable()
        {
            if (CurrentProgress > 0)
            {
                OnDecreaseLifeCountEvent?.Invoke();
            }

            OnUpdateEnemyCountEvent?.Invoke();
            OnUpdateEnemyCountEvent = null;
            OnDecreaseLifeCountEvent = null;
            OnDieEvent = null;
        }

        public void Damage(float amount)
        {
            if (IsDead) return;
            CurrentProgress -= amount;

            if (CurrentProgress > 0f) return;
            _collider.enabled = false;
            OnDieEvent?.Invoke();
        }

        public void Heal(float amount)
        {
            CurrentProgress += amount;

            if (CurrentProgress > Initial)
            {
                CurrentProgress = Initial;
            }
        }
    }
}