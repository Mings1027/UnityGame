using DG.Tweening;
using Unity.Mathematics;
using UnityEngine;

namespace StatusControl
{
    public class HealthBar : StatusBar
    {
        private Tween _shakeBarTween;
        private Health _health;

        [SerializeField] private float duration;
        [SerializeField] private float strength;
        [SerializeField] private int vibrato;
        [SerializeField] private float randomness;

        protected override void Awake()
        {
            base.Awake();
            _shakeBarTween = transform
                .DOShakeRotation(duration, new Vector3(0, 0, strength), vibrato, randomness, true,
                    ShakeRandomnessMode.Harmonic).SetUpdate(true)
                .OnComplete(() => transform.localRotation = quaternion.identity).SetAutoKill(false);
        }

        private void OnDestroy()
        {
            _shakeBarTween?.Kill();
        }

        public override void Init(Progressive progress)
        {
            base.Init(progress);
            _health = (Health)progressive;
            _health.OnShakeEvent += ShakeBarEvent;
        }

        public override void RemoveEvent(bool removeDirectly)
        {
            base.RemoveEvent(removeDirectly);
            _health.OnShakeEvent -= ShakeBarEvent;
        }

        private void ShakeBarEvent() => _shakeBarTween.Restart();
    }
}