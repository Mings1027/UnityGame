using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace PoolObjectControl
{
    public class VfxPoolObject : MonoBehaviour
    {
        private ParticleSystem _particleSystem;
        private float _particleLifeTime;

        private void Awake()
        {
            _particleSystem = GetComponent<ParticleSystem>();
            var mainModule = _particleSystem.main;
            _particleLifeTime = mainModule.startLifetime.constant + 1;
        }

        private async UniTaskVoid OnEnable()
        {
            _particleSystem.Play();
            await UniTask.Delay(TimeSpan.FromSeconds(_particleLifeTime));
            gameObject.SetActive(false);
        }
    }
}