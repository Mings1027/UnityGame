using GameControl;
using UnityEngine;
using UnityEngine.Rendering;

namespace ProjectileControl
{
    public class FollowProjectile : MonoBehaviour
    {
        private ParticleSystem _particleSystem;
        private bool _isTargeting;
        private string _hitVfxName;

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
                ObjectPoolManager.Get(_hitVfxName, target.position);
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

        public void SetHitVfx(string vfxName)
        {
            _hitVfxName = vfxName;
        }
    }
}