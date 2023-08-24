using GameControl;
using InterfaceControl;
using UnityEngine;

namespace ProjectileControl
{
    public abstract class Projectile : MonoBehaviour
    {
        private Vector3 _curPos;
        private Vector3 _startPos;
        private float _lerp;

        protected int damage;
        protected Transform target;
        protected Vector3 targetEndPos;

        [SerializeField] private AnimationCurve curve;
        [SerializeField] private float speed;

        protected virtual void OnEnable()
        {
            _lerp = 0;
            _startPos = transform.position;
        }

        protected abstract void FixedUpdate();

        protected virtual void OnTriggerEnter(Collider other)
        {
            ProjectileHit(other);
            gameObject.SetActive(false);
        }

        protected void ParabolaPath(Vector3 endPos)
        {
            var gravity = _lerp < 0.5f ? 1f : 1.2f;
            _lerp += Time.deltaTime * gravity * speed;
            _curPos = Vector3.Lerp(_startPos, endPos, _lerp);
            _curPos.y += curve.Evaluate(_lerp);
            var t = transform;
            var dir = (_curPos - t.position).normalized;
            if (dir == Vector3.zero) dir = t.forward;
            t.position = _curPos;
            t.forward = dir;
        }

        protected abstract void ProjectileHit(Collider col);

        protected void ApplyDamage(Collider col)
        {
            if (col.TryGetComponent(out IDamageable damageable))
            {
                damageable.Damage(damage);
            }
        }
    }
}