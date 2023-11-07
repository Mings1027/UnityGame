using DG.Tweening;
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
            _particleLifeTime = mainModule.startLifetime.constant;
        }

        private void OnEnable()
        {
            _particleSystem.Play();
            DOVirtual.DelayedCall(_particleLifeTime, () => gameObject.SetActive(false));
        }
    }
}