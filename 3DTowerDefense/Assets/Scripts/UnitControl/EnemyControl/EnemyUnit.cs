using System;
using AttackControl;
using UnityEngine;

namespace UnitControl.EnemyControl
{
    public abstract class EnemyUnit : Unit
    {
        protected Transform destination;

        public event Action onWaveEndedEvent;

        protected override void OnDisable()
        {
            base.OnDisable();
            onWaveEndedEvent?.Invoke();
            onWaveEndedEvent = null;
        }

        public void SetDestination(Transform pos)
        {
            destination = pos;
        }
    }
}