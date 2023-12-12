using System;
using UnityEngine;

namespace StatusControl
{
    [DisallowMultipleComponent]
    public abstract class Progressive : MonoBehaviour
    {
        private float _current;

        public float Current
        {
            get => _current;
            protected set
            {
                _current = value;
                OnUpdateBarEvent?.Invoke();
            }
        }

        protected float Initial { get; private set; }
        protected bool IsFull => _current >= Initial;
        public float Ratio => _current / Initial;

        public event Action OnUpdateBarEvent;

        protected virtual void OnEnable()
        {
            _current = Initial;
        }

        protected virtual void OnDisable()
        {
            OnUpdateBarEvent = null;
        }

        public virtual void Init(float amount)
        {
            Initial = amount;
            Current = amount;
        }
    }
}