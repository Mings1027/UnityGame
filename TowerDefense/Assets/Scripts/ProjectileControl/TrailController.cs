using DG.Tweening;
using UnityEngine;

namespace ProjectileControl
{
    public class TrailController : MonoBehaviour
    {
        private ParticleSystem _particleSystem;
        private ParticleSystem.ColorOverLifetimeModule _projectileColor;
        private Transform _projectileTransform;
        private Tween _destroyTween;
        private bool _isConnect;
        private const byte Interval = 3;

        private void Awake()
        {
            _particleSystem = GetComponent<ParticleSystem>();
            _projectileColor = _particleSystem.colorOverLifetime;
            _destroyTween = DOVirtual.DelayedCall(_particleSystem.main.duration,
                () => gameObject.SetActive(false), false).SetAutoKill(false).Pause();
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

        public void SpawnProjectile(Transform projectileTransform, ParticleSystem.MinMaxGradient projectileColor)
        {
            _isConnect = true;
            _destroyTween.Restart();
            _projectileTransform = projectileTransform;
            _projectileColor.color = projectileColor;
            _particleSystem.Play();
        }

        public void DisconnectProjectile() => _isConnect = false;
    }
}