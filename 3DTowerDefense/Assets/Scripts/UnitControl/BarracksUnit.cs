using System;

namespace UnitControl
{
    public class BarracksUnit : Unit
    {
        public event Action OnDeadEvent;

        protected override void OnDisable()
        {
            base.OnDisable();
            OnDeadEvent?.Invoke();
            OnDeadEvent = null;
        }

        protected override void Attack()
        {
            throw new NotImplementedException();
        }
    }
}