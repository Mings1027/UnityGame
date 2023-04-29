using System;
using GameControl;
using UnityEngine;

namespace BulletEffectControl
{
    public class BulletVFX : MonoBehaviour
    {
        private ParticleSystem _particle;

        private void Awake()
        {
            _particle = GetComponent<ParticleSystem>();
        }

        private void OnEnable()
        {
            _particle.Play();
            Invoke(nameof(DestroyEffect), 1);
        }

        private void OnDisable()
        {
            CancelInvoke();
            StackObjectPool.ReturnToPool(gameObject);
        }

        private void DestroyEffect()
        {
            gameObject.SetActive(false);
        }
    }
}