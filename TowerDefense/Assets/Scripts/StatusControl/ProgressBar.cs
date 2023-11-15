using UnityEngine;
using UnityEngine.UI;

namespace StatusControl
{
    public abstract class ProgressBar : MonoBehaviour
    {
        protected Progressive progressive;
        protected Slider slider { get; private set; }

        protected virtual void Awake()
        {
            slider = GetComponent<Slider>();
        }

        public virtual void Init(Progressive progress)
        {
            progressive = progress;
            progressive.OnUpdateBarEvent += UpdateBarEvent;
        }

        public virtual void RemoveEvent()
        {
            if (!progressive) return;
            progressive.OnUpdateBarEvent -= UpdateBarEvent;
        }

        private void UpdateBarEvent()
        {
            slider.value = progressive.Ratio;
        }
    }
}