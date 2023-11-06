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
            progressive = GetComponentInParent<Progressive>();
            slider = GetComponent<Slider>();
        }

        protected virtual void OnEnable()
        {
            progressive.OnUpdateBarEvent += UpdateBarEvent;
        }

        protected virtual void OnDisable()
        {
            progressive.OnUpdateBarEvent -= UpdateBarEvent;
        }

        private void UpdateBarEvent()
        {
            slider.value = progressive.Ratio;
        }
    }
}