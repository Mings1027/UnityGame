using System;
using DG.Tweening;
using GameControl;
using UnityEngine;

namespace BulletControl
{
    public class Bullet : MonoBehaviour
    {
        private Rigidbody _rigid;

        private float _lerp;
        private int _damage;
        private Transform _target;

        [SerializeField] private float bulletSpeed;

        private void Awake()
        {
            _rigid = GetComponent<Rigidbody>();
        }

        private void OnEnable()
        {
            Invoke(nameof(DestroyBullet), 3);
            _lerp = 0;
        }

        private void OnDisable()
        {
            CancelInvoke();
            StackObjectPool.ReturnToPool(gameObject);
        }

        private void FixedUpdate()
        {
            Shoot();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Enemy"))
            {
                Hit(other);
                DestroyBullet();
            }
        }

        private void Shoot()
        {
            if (_lerp > 1) return;
            _lerp += bulletSpeed * Time.deltaTime;
            _rigid.position = Vector3.Lerp(_rigid.position, _target.position, _lerp);
        }

        private void Hit(Component col)
        {
            if (col.TryGetComponent(out Health health))
            {
                health.TakeDamage(_damage, gameObject);
            }
        }

        private void DestroyBullet()
        {
            gameObject.SetActive(false);
        }

        public void Init(int damage, Transform target)
        {
            _damage = damage;
            _target = target;
        }
    }
}