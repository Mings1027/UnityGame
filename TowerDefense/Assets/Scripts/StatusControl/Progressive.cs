using System;
using UnityEngine;

namespace StatusControl
{
    [DisallowMultipleComponent]
    public abstract class Progressive : MonoBehaviour
    {
        private float _current;

        protected float Current
        {
            get => _current;
            set
            {
                _current = value;
                OnUpdateBarEvent?.Invoke();
            }
        }

        protected float Initial { get; private set; }

        public float Ratio => _current / Initial;

        public event Action OnUpdateBarEvent;

        protected virtual void OnEnable()
        {
            _current = Initial;
        }

        public void Init(float amount)
        {
            Initial = amount;
            Current = amount;
        }
    }
}