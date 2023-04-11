using GameControl;
using UnityEngine;

namespace WeaponControl
{
    public abstract class Projectile : MonoBehaviour
    {
        private float lerp;
        private Vector3 _curPos;
        private Vector3 _startPos;

        protected int _damage;
        protected Vector3 _endPos;

        protected float ProjectileSpeed => speed;
        protected Transform target;

        [SerializeField] private string effect, explosion;
        [SerializeField] private string tagName;
        [SerializeField] private AnimationCurve curve;
        [SerializeField] private float speed;

        protected virtual void OnEnable()
        {
            lerp = 0;
            _startPos = transform.position;
        }

        private void FixedUpdate()
        {
            ProjectilePath();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag(tagName))
            {
                var pos = transform.position;
                StackObjectPool.Get(effect, pos);
                StackObjectPool.Get(explosion, pos);
                ProjectileHit(other);
                gameObject.SetActive(false);
            }
        }

        protected virtual void ProjectilePath()
        {
            var gravity = lerp < 0.5f ? 1 : 1.5f;
            lerp += Time.fixedDeltaTime * gravity * speed;
            _curPos = Vector3.Lerp(_startPos, _endPos, lerp);
            _curPos.y += curve.Evaluate(lerp);
            var t = transform;
            var dir = (_curPos - t.position).normalized;
            t.position = _curPos;
            t.forward = dir;
        }

        protected virtual void ProjectileHit(Collider col)
        {
            Damage(col);
        }

        protected void Damage(Collider col)
        {
            if (col.TryGetComponent(out Health h))
            {
                h.TakeDamage(_damage, col.gameObject);
            }
        }

        public abstract void Init(Transform t, int damage);
        
    }
}