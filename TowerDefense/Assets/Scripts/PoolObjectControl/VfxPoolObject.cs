using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace PoolObjectControl
{
    public class VfxPoolObject : MonoBehaviour
    {
        private ParticleSystem _particleSystem;
        private float _maxDurationTime;

        private void Awake()
        {
            _particleSystem = GetComponent<ParticleSystem>();
            _maxDurationTime = _particleSystem.main.duration;
        }

        private void OnEnable()
        {
            DisableObject().Forget();
        }

        private async UniTaskVoid DisableObject()
        {
            _particleSystem.Play();

            await UniTask.Delay(TimeSpan.FromSeconds(_maxDurationTime),
                cancellationToken: this.GetCancellationTokenOnDestroy());
            gameObject.SetActive(false);
        }
    }
}