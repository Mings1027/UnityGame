using UnityEngine;

namespace PoolObjectControl
{
    public class VfxPoolObject : MonoBehaviour
    {
        private ParticleSystem _particleSystem;

        private void Awake()
        {
            _particleSystem = GetComponent<ParticleSystem>();
        }

        private void OnEnable()
        {
            _particleSystem.Play();
        }

        private void Update()
        {
            if (!_particleSystem.isPlaying)
            {
                gameObject.SetActive(false);
            }
        }
    }
}