using System;
using UnityEngine;

namespace StatusControl
{
    [DisallowMultipleComponent]
    public abstract class Progressive : MonoBehaviour
    {
        protected float current;

        protected float Current
        {
            get => current;
            set
            {
                current = value;
                OnUpdateBarEvent?.Invoke();
            }
        }

        protected float Initial { get; private set; }

        public float Ratio => current / Initial;

        public event Action OnUpdateBarEvent;

        protected virtual void OnEnable()
        {
            current = Initial;
        }

        public void Init(float amount)
        {
            Initial = amount;
            Current = amount;
        }
    }
}