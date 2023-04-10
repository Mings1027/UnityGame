using GameControl;
using UnityEngine;

namespace WeaponControl
{
    public abstract class Projectile : MonoBehaviour
    {
        private Transform _endPos;
        private Vector3 _curPos, _startPos;
        private int _damage;

        private float lerp;

        protected float ProjectileSpeed => speed;
        public Transform Target { get; set; }

        [SerializeField] private string effect, explosion;
        [SerializeField] private string tagName;
        [SerializeField] private AnimationCurve curve;
        [SerializeField] private float speed;

        protected virtual void OnEnable()
        {
            lerp = 0;
            _startPos = transform.position;
        }

        protected virtual void FixedUpdate()
        {
            var gravity = lerp < 0.5f ? 1 : 1.5f;
            lerp += Time.fixedDeltaTime * gravity * speed;
            _curPos = Vector3.Lerp(_startPos, _endPos.position, lerp);
            Parabola();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag(tagName))
            {
                HitEffect(other);
            }
        }

        private void Parabola()
        {
            _curPos.y += curve.Evaluate(lerp);
            var t = transform;
            var dir = (_curPos - t.position).normalized;
            t.position = _curPos;
            t.forward = dir;
        }

        protected virtual void HitEffect(Collider col)
        {
            var pos = transform.position;
            StackObjectPool.Get(effect, pos);
            StackObjectPool.Get(explosion, pos);
        }

        protected void GetDamage(Collider col)
        {
            if (col.TryGetComponent(out Health h))
            {
                h.GetHit(_damage, col.gameObject);
            }
        }

        public void Setting(Transform endPos, int damage)
        {
            _endPos = endPos;
            _damage = damage;
        }

        public void Init(Transform target, int damage)
        {
            Target = target;
            _damage = damage;
        }
    }
}