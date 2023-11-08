using UnityEngine;
using UnityEngine.UI;

namespace StatusControl
{
    public abstract class ProgressBar : MonoBehaviour
    {
        protected Progressive Progressive;
        protected Slider slider { get; private set; }

        protected virtual void Awake()
        {
            slider = GetComponent<Slider>();
        }

        public virtual void Init(Progressive progressive)
        {
            Progressive = progressive;
            Progressive.OnUpdateBarEvent += UpdateBarEvent;
        }

        private void UpdateBarEvent()
        {
            slider.value = Progressive.Ratio;
        }
    }
}