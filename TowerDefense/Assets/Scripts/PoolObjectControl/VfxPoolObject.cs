using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace PoolObjectControl
{
    public class VfxPoolObject : MonoBehaviour
    {
        private ParticleSystem _particleSystem;
        private float particleLifeTime;

        private void Awake()
        {
            _particleSystem = GetComponent<ParticleSystem>();
            var mainModule = _particleSystem.main;
            particleLifeTime = mainModule.startLifetime.constant;
        }

        private void OnEnable()
        {
            _particleSystem.Play();
            DisableParticle().Forget();
        }

        private async UniTaskVoid DisableParticle()
        {
            await UniTask.Delay(TimeSpan.FromSeconds(particleLifeTime));
            gameObject.SetActive(false);
        }
    }
}