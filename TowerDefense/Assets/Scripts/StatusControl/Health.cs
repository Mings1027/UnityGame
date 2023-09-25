using System;
using DG.Tweening;
using InterfaceControl;
using UnityEngine;

namespace StatusControl
{
    public class Health : Progressive, IDamageable, IHealable
    {
        private ParticleSystem _bloodParticle;
        private Collider _collider;

        public event Action OnDeadEvent;

        private void Awake()
        {
            _bloodParticle = GetComponentInChildren<ParticleSystem>();
            _collider = GetComponent<Collider>();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            _collider.enabled = true;
        }

        public void Damage(in float amount)
        {
            if (IsDead) return;
            CurrentProgress -= amount;

            _bloodParticle.Play();
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