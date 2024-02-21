using DG.Tweening;
using UnityEngine;

namespace ProjectileControl
{
    public class TrailController : MonoBehaviour
    {
        private ParticleSystem _particleSystem;
        private Transform _projectileTransform;
        private Tween _destroyTween;
        private bool _isConnect;
        private const byte Interval = 3;

        public ParticleSystem.ColorOverLifetimeModule colorOverLifetime { get; private set; }

        private void Awake()
        {
            _particleSystem = GetComponent<ParticleSystem>();
            colorOverLifetime = _particleSystem.colorOverLifetime;
            _destroyTween = DOVirtual.DelayedCall(_particleSystem.main.duration,
                () => gameObject.SetActive(false)).SetAutoKill(false).Pause();
        }

        private void OnDestroy()
        {
            _destroyTween?.Kill();
        }

        private void LateUpdate()
        {
            if (!_isConnect) return;
            if (Time.frameCount % Interval == 0)
            {
                transform.position = _projectileTransform.position;
            }
        }

        public void SetProjectileTransform(Transform projectileTransform)
        {
            _isConnect = true;
            _destroyTween.Restart();
            _projectileTransform = projectileTransform;
            _particleSystem.Play();
        }

        public void DisconnectProjectile() => _isConnect = false;
    }
}