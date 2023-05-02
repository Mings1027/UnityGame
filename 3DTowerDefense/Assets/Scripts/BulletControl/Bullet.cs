using GameControl;
using UnityEngine;

namespace BulletControl
{
    public class Bullet : MonoBehaviour
    {
        private Rigidbody _rigid;

        public int damage;
        [SerializeField] private float bulletSpeed;

        private void Awake()
        {
            _rigid = GetComponent<Rigidbody>();
        }

        private void OnEnable()
        {
            Invoke(nameof(DestroyBullet), 3);
        }

        private void OnDisable()
        {
            CancelInvoke();
            StackObjectPool.ReturnToPool(gameObject);
        }

        private void FixedUpdate()
        {
            _rigid.MovePosition(_rigid.position + _rigid.transform.forward * (bulletSpeed * Time.deltaTime));
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Enemy"))
            {
                Hit(other);
                DestroyBullet();
            }
        }

        private void Hit(Component col)
        {
            if (col.TryGetComponent(out Health health))
            {
                health.TakeDamage(damage, gameObject);
            }
        }

        private void DestroyBullet()
        {
            gameObject.SetActive(false);
        }
    }
}