using System;
using InterfaceControl;
using UnityEngine;

namespace StatusControl
{
    public class Health : Progressive, IDamageable, IHealable
    {
        public event Action OnDie;

        public void Damage(float amount)
        {
            CurrentProgress -= amount;

            if (CurrentProgress > 0f) return;
            
            OnDie?.Invoke();
            OnDie = null;
            gameObject.SetActive(false);
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