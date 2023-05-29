using System;
using GameControl;
using UnityEngine;

namespace UnitControl.EnemyControl
{
    public abstract class EnemyUnit : Unit
    {
        protected Transform destination;
        private Health _health;

        public event Action onDeadEvent;
        public event Action onCoinEvent;
        public event Action onLifeCountEvent;

        protected override void Awake()
        {
            base.Awake();
            _health = GetComponent<Health>();
        }

        public void SetDestination(Transform pos)
        {
            destination = pos;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            onDeadEvent?.Invoke();
            if (_health.CurHealth > 0) onLifeCountEvent?.Invoke();
            else onCoinEvent?.Invoke();

            onDeadEvent = null;
            onLifeCountEvent = null;
            onCoinEvent = null;
        }
    }
}