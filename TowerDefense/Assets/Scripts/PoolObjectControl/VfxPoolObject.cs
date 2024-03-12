using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace PoolObjectControl
{
    public class VfxPoolObject : MonoBehaviour
    {
        private ParticleSystem[] _particleSystem;
        private float _maxStartLifeTime;

        private void Awake()
        {
            _maxStartLifeTime = GetMaxStartLifeTime();
        }

        private void OnEnable()
        {
            DisableObject().Forget();
        }

        private async UniTaskVoid DisableObject()
        {
            for (var i = 0; i < _particleSystem.Length; i++)
            {
                _particleSystem[i].Play();
            }

            await UniTask.Delay(TimeSpan.FromSeconds(_maxStartLifeTime),
                cancellationToken: this.GetCancellationTokenOnDestroy());
            gameObject.SetActive(false);
        }

        private float GetMaxStartLifeTime()
        {
            _particleSystem = GetComponentsInChildren<ParticleSystem>();
            var maxStartLifeTime = 0f;
            for (int i = 0; i < _particleSystem.Length; i++)
            {
                maxStartLifeTime = Mathf.Max(maxStartLifeTime, _particleSystem[i].main.startLifetime.constant);
            }

            return maxStartLifeTime;
        }
    }
}