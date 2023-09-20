using GameControl;
using PoolObjectControl;
using UnityEngine;

namespace ProjectileControl
{
    public class FollowProjectile : MonoBehaviour
    {
        private ParticleSystem _particleSystem;
        private Transform _followTarget;
        private bool _isTargeting;
        private string _hitVfxName;

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
            if (!_followTarget.gameObject.activeSelf)
            {
                _isTargeting = false;
                ObjectPoolManager.Get(_hitVfxName, _followTarget.position);
                return;
            }

            transform.position = _followTarget.position;
        }

        private void OnDisable()
        {
            _followTarget = null;
            _isTargeting = false;
            _particleSystem.Stop();
        }

        public void Init(Transform t, string vfxName)
        {
            _followTarget = t;
            _hitVfxName = vfxName;
        }
    }
}