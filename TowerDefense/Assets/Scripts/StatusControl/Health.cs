using System;
using DG.Tweening;
using InterfaceControl;

namespace StatusControl
{
    public class Health : Progressive, IDamageable, IHealable
    {
        public event Action OnDeadEvent;
        
        public void Damage(float amount)
        {
            CurrentProgress -= amount;

            if (CurrentProgress > 0f) return;
            OnDeadEvent?.Invoke();
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