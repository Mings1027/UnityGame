using DG.Tweening;
using UnityEngine;

namespace PoolObjectControl
{
    public class VfxPoolObject : MonoBehaviour
    {
        private ParticleSystem _particleSystem;
        private Tween _disableTween;

        private void Awake()
        {
            _particleSystem = GetComponent<ParticleSystem>();
            var mainModule = _particleSystem.main;
            _disableTween = DOVirtual
                .DelayedCall(mainModule.startLifetime.constant, () => gameObject.SetActive(false), false)
                .SetAutoKill(false).Pause();
        }

        private void OnEnable()
        {
            _particleSystem.Play();
            _disableTween.Restart();
        }

        private void OnDestroy()
        {
            _disableTween?.Kill();
        }
    }
}