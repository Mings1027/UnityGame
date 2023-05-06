using GameControl;
using UnityEngine;

namespace BulletControl
{
    public class Bullet : MonoBehaviour
    {
        private Rigidbody _rigid;

        private float _lerp;
        private int _damage;
        private Vector3 _startPos;
        private Transform _targetPos;

        [SerializeField] private float bulletSpeed;

        private void Awake()
        {
            _rigid = GetComponent<Rigidbody>();
        }

        private void OnEnable()
        {
            Invoke(nameof(DestroyBullet), 2);
            _lerp = 0;
            _startPos = _rigid.position;
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
            }

            DestroyBullet();
        }

        private void Shoot()
        {
            if (_lerp > 1) return;
            _lerp += bulletSpeed * Time.deltaTime;
            _rigid.position = Vector3.Lerp(_startPos, _targetPos.position + new Vector3(0, 0.5f, 0), _lerp);
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

        public void Init(int damage, Transform targetPos)
        {
            _damage = damage;
            _targetPos = targetPos;
        }
    }
}