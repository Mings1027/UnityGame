using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ProjectileControl
{
    public class TrailController : MonoBehaviour
    {
        private ParticleSystem _particleSystem;
        private ParticleSystem.ColorOverLifetimeModule _projectileColor;

        private Transform _projectileTransform;

        private float _startLifeTime;
        private bool _isConnect;
        private const byte Interval = 3;

        private void Awake()
        {
            _particleSystem = GetComponent<ParticleSystem>();
            _projectileColor = _particleSystem.colorOverLifetime;
            _startLifeTime = _particleSystem.main.duration;
        }

        private void OnEnable()
        {
            DisableObject().Forget();
        }

        private void LateUpdate()
        {
            if (!_isConnect) return;
            if (Time.frameCount % Interval == 0)
            {
                transform.position = _projectileTransform.position;
            }
        }

        private async UniTaskVoid DisableObject()
        {
            _particleSystem.Play();
            await UniTask.Delay(TimeSpan.FromSeconds(_startLifeTime),
                cancellationToken: this.GetCancellationTokenOnDestroy());
            gameObject.SetActive(false);
        }

        public void SpawnProjectile(Transform projectileTransform, ParticleSystem.MinMaxGradient projectileColor)
        {
            _isConnect = true;
            _projectileTransform = projectileTransform;
            _projectileColor.color = projectileColor;
            _particleSystem.Play();
        }

        public void DisconnectProjectile() => _isConnect = false;
    }
}