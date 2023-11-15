using DG.Tweening;
using Unity.Mathematics;
using UnityEngine;

namespace StatusControl
{
    public class HealthBar : ProgressBar
    {
        private Transform _border;

        [SerializeField] private float duration;
        [SerializeField] private float strength;
        [SerializeField] private int vibrato;
        [SerializeField] private float randomness;

        protected override void Awake()
        {
            base.Awake();
            _border = transform.GetChild(0);
        }

        public override void Init(Progressive progress)
        {
            base.Init(progress);
            progressive.OnUpdateBarEvent += ShakeBarEvent;
        }

        public override void RemoveEvent()
        {
            base.RemoveEvent();
            progressive.OnUpdateBarEvent -= ShakeBarEvent;
        }

        private void ShakeBarEvent() => _border
            .DOShakeRotation(duration, new Vector3(0, 0, strength), vibrato, randomness, true,
                ShakeRandomnessMode.Harmonic)
            .OnComplete(() => _border.localRotation = quaternion.identity);
    }
}