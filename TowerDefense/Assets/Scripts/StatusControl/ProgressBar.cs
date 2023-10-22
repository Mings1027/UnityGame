using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace StatusControl
{
    public abstract class ProgressBar : MonoBehaviour
    {
        protected Progressive progressive;
        protected Image FillImage => fillImage;
        
        [SerializeField] private Image fillImage;

        protected virtual void Awake()
        {
            progressive = GetComponentInParent<Progressive>();
        }

        protected virtual void OnEnable()
        {
            progressive.OnUpdateBarEvent += UpdateBarEvent;
        }

        protected virtual void OnDisable()
        {
            progressive.OnUpdateBarEvent -= UpdateBarEvent;
        }

        private void UpdateBarEvent() => fillImage.fillAmount = progressive.Ratio;
    }
}