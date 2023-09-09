using System;
using UnityEngine;

namespace StatusControl
{
    [DisallowMultipleComponent]
    public abstract class Progressive : MonoBehaviour
    {
        private float _currentProgress;

        protected float CurrentProgress
        {
            get => _currentProgress;
            set
            {
                _currentProgress = value;
                OnUpdateBarEvent?.Invoke();
            }
        }

        protected float Initial { get; private set; }

        public float Ratio => _currentProgress / Initial;
        public bool IsDead => _currentProgress <= 0;

        public event Action OnUpdateBarEvent;

        protected virtual void OnEnable()
        {
            CurrentProgress = Initial;
        }

        public void Init(float amount)
        {
            Initial = amount;
            CurrentProgress = amount;
        }
    }
}