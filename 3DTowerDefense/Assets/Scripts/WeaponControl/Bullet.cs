using GameControl;
using UnityEngine;

namespace WeaponControl
{
    public abstract class Bullet : MonoBehaviour
    {
        private int _damage;

        protected Transform target;
        protected Rigidbody rigid;
        protected float BulletSpeed => bulletSpeed;

        [SerializeField] private float bulletSpeed;

        protected virtual void Awake()
        {
            rigid = GetComponent<Rigidbody>();
        }

        protected virtual void OnEnable()
        {
        }

        private void FixedUpdate()
        {
            AttackPath();
        }

        protected virtual void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Enemy")) return;
            BulletHit(other);
            gameObject.SetActive(false);
        }

        protected abstract void AttackPath();

        protected virtual void BulletHit(Component other)
        {
            if (other.TryGetComponent(out Health h))
            {
                h.TakeDamage(_damage);
            }
        }

        public void Init(Transform t, int d)
        {
            target = t;
            _damage = d;
        }
    }
}