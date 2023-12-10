using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace StatusControl
{
    public abstract class StatusBar : MonoBehaviour
    {
        private Tweener _disableTween;

        protected Progressive progressive;
        protected Slider slider { get; private set; }

        protected virtual void Awake()
        {
            slider = GetComponent<Slider>();
            _disableTween = transform.DOScale(0, 0.5f).SetAutoKill(false).Pause()
                .OnComplete(() =>
                {
                    gameObject.SetActive(false);
                    transform.localScale = Vector3.one;
                });
        }

        public virtual void Init(Progressive progress)
        {
            progressive = progress;
            progressive.OnUpdateBarEvent += UpdateBarEvent;
        }

        public virtual void RemoveEvent(bool removeDirectly)
        {
            if (!progressive) return;
            progressive.OnUpdateBarEvent -= UpdateBarEvent;
            if (removeDirectly) gameObject.SetActive(false);
            else _disableTween.ChangeStartValue(transform.localScale).Restart();
        }

        private void UpdateBarEvent()
        {
            slider.value = progressive.Ratio;
        }
    }
}