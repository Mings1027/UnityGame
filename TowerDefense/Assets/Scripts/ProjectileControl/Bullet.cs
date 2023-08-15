using GameControl;
using UnityEngine;

namespace ProjectileControl
{
    public abstract class Bullet : MonoBehaviour
    {
        private Transform _target;
        private int _damage;

        protected Rigidbody rigid;
        protected MeshRenderer meshRenderer;
        protected ParticleSystem particle;
        protected float BulletSpeed => bulletSpeed;

        [SerializeField] private float bulletSpeed;

        protected virtual void Awake()
        {
            rigid = GetComponent<Rigidbody>();
            meshRenderer = transform.GetChild(0).GetComponent<MeshRenderer>();
            particle = transform.GetChild(1).GetComponent<ParticleSystem>();
        }

        protected virtual void OnEnable()
        {
            meshRenderer.enabled = true;
        }

        private void FixedUpdate()
        {
            AttackPath();
        }

        protected virtual void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Enemy"))
            {
                BulletHit(other);
            }

            rigid.velocity = Vector3.zero;
            meshRenderer.enabled = false;
            particle.Stop();
        }

        protected virtual void AttackPath()
        {
            var dir = (_target.position - rigid.position).normalized;
            rigid.velocity = dir * (BulletSpeed * Time.fixedDeltaTime);
            rigid.MoveRotation(Quaternion.LookRotation(dir));
        }

        protected virtual void BulletHit(Component other)
        {
            if (other.TryGetComponent(out EnemyHealth h))
            {
                h.TakeDamage(_damage);
            }
        }

        public void Init(Transform t, int d)
        {
            _target = t;
            _damage = d;
        }
    }
}