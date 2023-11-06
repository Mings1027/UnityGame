using DG.Tweening;
using UnityEngine;

namespace StatusControl
{
    public class HealthBar : ProgressBar
    {
        private Transform border;

        [SerializeField] private float duration;
        [SerializeField] private float strength;
        [SerializeField] private int vibrato;

        protected override void Awake()
        {
            base.Awake();
            border = transform.GetChild(0);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            progressive.OnUpdateBarEvent += ShakeBarEvent;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            progressive.OnUpdateBarEvent -= ShakeBarEvent;
        }

        private void ShakeBarEvent() => border.DOShakePosition(duration, strength, vibrato)
            .OnComplete(() => border.localPosition = Vector3.one);
    }
}