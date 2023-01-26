using Enemy;
using UnityEngine;
using UnityEngine.Pool;

namespace Player.PlayerAttack
{
    public class PlayerBullet : MonoBehaviour
    {
        private IObjectPool<PlayerBullet> releasePool;
        public Rigidbody rigid;
        private new ParticleSystem particleSystem;
        private int damage;
        [SerializeField] private float projectileSpeed, lifeTime;
        [SerializeField] private string projectile, hit;
        private Transform playerTransform;

        private void Awake()
        {
            playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
            particleSystem = GetComponent<ParticleSystem>();
            rigid = GetComponent<Rigidbody>();
        }

        public void Reset()
        {
            rigid.velocity = Vector3.zero;
            rigid.angularVelocity = Vector3.zero;
        }

        public void SetPool(IObjectPool<PlayerBullet> pool)
        {
            releasePool = pool;
        }

        private void OnEnable()
        {
            rigid.transform.forward = playerTransform.forward;
            rigid.transform.rotation = playerTransform.rotation;
            particleSystem.Play();
            rigid.AddForce(rigid.transform.forward * projectileSpeed, ForceMode.VelocityChange);
            Invoke(nameof(DestroyBullet), lifeTime);
        }

        private void DestroyBullet()
        {
            releasePool.Release(this);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Enemy"))
                other.GetComponent<EnemyHealth>().OnDamage(damage, playerTransform);
            // DestroyBullet();
            gameObject.SetActive(false);
        }
    }
}