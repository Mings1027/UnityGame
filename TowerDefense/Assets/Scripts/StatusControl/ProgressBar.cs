using UnityEngine;
using UnityEngine.UI;

namespace StatusControl
{
    public abstract class ProgressBar : MonoBehaviour
    {
        protected Progressive progressive;
        protected Image FillImage { get; private set; }

        protected virtual void Awake()
        {
            progressive = GetComponentInParent<Progressive>();
            FillImage = transform.GetChild(0).GetChild(0).GetComponent<Image>();
        }

        protected virtual void OnEnable()
        {
            progressive.OnUpdateBarEvent += UpdateBarEvent;
        }

        protected virtual void OnDisable()
        {
            progressive.OnUpdateBarEvent -= UpdateBarEvent;
        }

        private void UpdateBarEvent() => FillImage.fillAmount = progressive.Ratio;
    }
}