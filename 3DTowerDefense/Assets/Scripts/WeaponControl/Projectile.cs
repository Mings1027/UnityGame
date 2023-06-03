using System;
using GameControl;
using UnityEngine;

namespace WeaponControl
{
    public abstract class Projectile : MonoBehaviour
    {
        private int _damage;
        private float _lerp;
        private Vector3 _curPos;
        private Vector3 _startPos;
        private Transform _target;

        [SerializeField] private AnimationCurve curve;
        [SerializeField] private float speed;

        protected virtual void Awake()
        {
        }

        protected virtual void OnEnable()
        {
            _lerp = 0;
            _startPos = transform.position;
        }

        protected abstract void FixedUpdate();

        protected abstract void OnTriggerEnter(Collider other);

        protected void ParabolaPath()
        {
            var gravity = _lerp < 0.5f ? 1f : 1.2f;
            _lerp += Time.fixedDeltaTime * gravity * speed;
            _curPos = Vector3.Lerp(_startPos, _target.position, _lerp);
            _curPos.y += curve.Evaluate(_lerp);
            var t = transform;
            var dir = (_curPos - t.position).normalized;
            if (dir == Vector3.zero) dir = t.forward;
            t.position = _curPos;
            t.forward = dir;
        }

        protected abstract void ProjectileHit(Collider col);

        protected void Damaging(Collider col)
        {
            if (col.TryGetComponent(out Health h))
            {
                h.TakeDamage(_damage, col.gameObject);
            }
        }

        public void Init(Transform target, int damage)
        {
            _target = target;
            _damage = damage;
        }
    }
}