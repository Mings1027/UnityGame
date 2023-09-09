using UnityEngine;
using Random = UnityEngine.Random;

namespace ProjectileControl
{
    public class FollowProjectile : MonoBehaviour
    {
        private bool _isTargeting;
        private ParticleSystem _particleSystem;

        public Transform target { get; set; }

        private void Awake()
        {
            _particleSystem = GetComponent<ParticleSystem>();
        }

        private void OnEnable()
        {
            _particleSystem.Play();
            _isTargeting = true;
        }

        private void FixedUpdate()
        {
            if (!_isTargeting) return;
            if (!target.gameObject.activeSelf)
            {
                _isTargeting = false;
                return;
            }

            transform.position = target.position;
        }

        private void OnDisable()
        {
            target = null;
            _isTargeting = false;
            _particleSystem.Stop();
        }
    }
}