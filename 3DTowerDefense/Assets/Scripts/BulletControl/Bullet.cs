using GameControl;
using UnityEngine;

namespace BulletControl
{
    public class Bullet : MonoBehaviour
    {
        private Rigidbody _rigid;

        private int _damage;
        // private float _lerp;
        // private Transform _targetPos;
        private Vector3 _dir;

        [SerializeField] private float bulletSpeed;

        private void Awake()
        {
            _rigid = GetComponent<Rigidbody>();
        }

        private void OnEnable()
        {
            Invoke(nameof(DestroyBullet), 2);
        }

        private void OnDisable()
        {
            CancelInvoke();
            StackObjectPool.ReturnToPool(gameObject);
        }

        private void FixedUpdate()
        {
            Shoot();
            // LerpShoot();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Enemy"))
            {
                Hit(other);
            }

            DestroyBullet();
        }

        private void Shoot()
        {
            _rigid.MovePosition(_rigid.position + _dir * (bulletSpeed * Time.deltaTime));
        }

        // private void LerpShoot()
        // {
        //     if (_lerp <= 1)
        //     {
        //         _rigid.position = Vector3.Lerp(_rigid.position, _targetPos.position, _lerp);
        //         _lerp += bulletSpeed * Time.deltaTime;
        //     }
        // }

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

        public void Init(int damage, Vector3 dir)
        {
            _damage = damage;
            // _targetPos = targetPos;
            _dir = dir;
            // _lerp = 0;
        }
    }
}