using System;
using UnityEngine;

namespace StatusControl
{
    [DisallowMultipleComponent]
    public abstract class Progressive : MonoBehaviour
    {
        private float currentProgress;

        protected float CurrentProgress
        {
            get => currentProgress;
            set
            {
                currentProgress = value;
                OnUpdateBar?.Invoke();
            }
        }

        protected float Initial { get; private set; }

        public float Ratio => currentProgress / Initial;

        public event Action OnUpdateBar;

        private void OnEnable()
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