using System;
using DG.Tweening;
using GameControl;
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

        private void Awake()
        {
            _collider = GetComponent<Collider>();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            _collider.enabled = true;
        }

        private void OnDisable()
        {
            if (transform.position == Vector3.zero)
            {
                OnDecreaseLifeCountEvent?.Invoke();
            }

            OnUpdateEnemyCountEvent?.Invoke();
            OnUpdateEnemyCountEvent = null;
            OnDecreaseLifeCountEvent = null;
            OnDieEvent = null;
            ObjectPoolManager.ReturnToPool(gameObject);
        }

        public void Damage(float amount)
        {
            CurrentProgress -= amount;

            if (CurrentProgress > 0f) return;
            _collider.enabled = false;
            OnDieEvent?.Invoke();
            DOVirtual.DelayedCall(1, () => gameObject.SetActive(false));
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