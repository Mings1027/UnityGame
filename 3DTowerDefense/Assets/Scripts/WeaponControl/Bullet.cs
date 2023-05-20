using DG.Tweening;
using GameControl;
using UnityEngine;

namespace WeaponControl
{
    public abstract class Bullet : MonoBehaviour
    {
        private int damage;
        private AudioSource audioSource;
        
        protected Transform target;
        protected Rigidbody rigid;
        protected float BulletSpeed => bulletSpeed;

        [SerializeField] private float bulletSpeed;

        private void Awake()
        {
            rigid = GetComponent<Rigidbody>();
        }

        protected virtual void OnEnable()
        {
            
        }

        private void FixedUpdate()
        {
            StraightPath();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Enemy"))
            {
                BulletHit(other);
                gameObject.SetActive(false);
            }
        }

        protected abstract void StraightPath();
        
        protected virtual void BulletHit(Component other)
        {
            if (other.TryGetComponent(out Health h))
            {
                h.TakeDamage(damage, other.gameObject);
            }
        }

        public void Init(Transform t, int d)
        {
            target = t;
            damage = d;
        }
    }
}