using GameControl;
using UnityEngine;

namespace WeaponControl
{
    public abstract class Projectile : MonoBehaviour
    {
        private int damage;
        private float lerp;
        private Vector3 curPos;
        private Vector3 startPos;
        private Transform target;
        private AudioSource audioSource;
        
        [SerializeField] private string tagName;
        [SerializeField] private AnimationCurve curve;
        [SerializeField] private float speed;
        [SerializeField] private AudioClip enableAudio;

        protected virtual void Awake()
        {
            audioSource = GetComponent<AudioSource>();
        }

        protected virtual void OnEnable()
        {
            lerp = 0;
            startPos = transform.position;

            audioSource.PlayOneShot(enableAudio);
        }

        protected abstract void FixedUpdate();

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag(tagName))
            {
                ProjectileHit(other);
                gameObject.SetActive(false);
            }
        }

        protected void ParabolaPath()
        {
            var gravity = lerp < 0.5f ? 1f : 1.2f;
            lerp += Time.fixedDeltaTime * gravity * speed;
            curPos = Vector3.Lerp(startPos, target.position, lerp);
            curPos.y += curve.Evaluate(lerp);
            var t = transform;
            var dir = (curPos - t.position).normalized;
            if (dir == Vector3.zero) dir = t.forward;
            t.position = curPos;
            t.forward = dir;
        }

        protected abstract void ProjectileHit(Collider col);

        protected void Damaging(Collider col)
        {
            if (col.TryGetComponent(out Health h))
            {
                h.TakeDamage(damage, col.gameObject);
            }
        }

        public void Init(Transform t, int d)
        {
            target = t;
            damage = d;
        }
    }
}