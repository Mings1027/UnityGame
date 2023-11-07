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

        protected override void OnEnable()
        {
            base.OnEnable();
            Progressive.OnUpdateBarEvent += ShakeBarEvent;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            Progressive.OnUpdateBarEvent -= ShakeBarEvent;
        }

        private void ShakeBarEvent() => _border
            .DOShakeRotation(duration, new Vector3(0, 0, strength), vibrato, randomness, true,
                ShakeRandomnessMode.Harmonic)
            .OnComplete(() => _border.localRotation = quaternion.identity);
    }
}