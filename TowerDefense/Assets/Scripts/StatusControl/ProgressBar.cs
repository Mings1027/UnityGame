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
            Progressive = GetComponentInParent<Progressive>();
            slider = GetComponent<Slider>();
        }

        protected virtual void OnEnable()
        {
            Progressive.OnUpdateBarEvent += UpdateBarEvent;
        }

        protected virtual void OnDisable()
        {
            Progressive.OnUpdateBarEvent -= UpdateBarEvent;
        }

        private void UpdateBarEvent()
        {
            slider.value = Progressive.Ratio;
        }
    }
}