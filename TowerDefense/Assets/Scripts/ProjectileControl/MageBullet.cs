using System;
using DataControl;
using GameControl;
using InterfaceControl;
using StatusControl;
using TowerControl;
using UnitControl.EnemyControl;
using UnityEngine;

namespace ProjectileControl
{
    public sealed class MageBullet : MonoBehaviour
    {
        private Transform _target;
        private int _damage;
        private Transform t;

        private SphereCollider sphereCollider;
        private ParticleSystem particle;
        private SpeedDeBuffData _speedDeBuffData;

        private float timeSinceLastUpdate;

        [SerializeField] private float bulletSpeed;
        [SerializeField] private float updateInterval;

        private void Awake()
        {
            t = transform;
            sphereCollider = GetComponent<SphereCollider>();
            particle = GetComponentInChildren<ParticleSystem>();
        }

        private void OnEnable()
        {
            sphereCollider.enabled = true;
        }

        private void Update()
        {
            ProjectilePath();
        }

        private void OnTriggerEnter(Collider other)
        {
            sphereCollider.enabled = false;
            particle.Stop();
         
            WhenHitEnemy(other);
        }

        private void ProjectilePath()
        {
            timeSinceLastUpdate += Time.deltaTime;
            if (timeSinceLastUpdate < updateInterval) return;

            var dir = (_target.position - t.position).normalized;
            transform.position += dir * (bulletSpeed * timeSinceLastUpdate);
            timeSinceLastUpdate = 0;
        }

        public void Init(Transform target, int damage, SpeedDeBuffData speedDeBuffData)
        {
            _target = target;
            _damage = damage;
            _speedDeBuffData = speedDeBuffData;
        }

        private void WhenHitEnemy(Component other)
        {
            if (other.TryGetComponent(out IDamageable damageable))
            {
                damageable.Damage(_damage);
            }

            if (other.TryGetComponent(out EnemyStatus e))
            {
                e.SlowEffect(_speedDeBuffData).Forget();
            }
        }
    }
}