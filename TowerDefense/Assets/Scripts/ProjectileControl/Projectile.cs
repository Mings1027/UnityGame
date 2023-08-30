using System;
using InterfaceControl;
using ManagerControl;
using UnityEngine;

namespace ProjectileControl
{
    public abstract class Projectile : MonoBehaviour
    {
        private Rigidbody _rigid;
        private MeshRenderer _meshRenderer;
        private SphereCollider _sphereCollider;
        private Vector3 _curPos;
        private Vector3 _startPos;
        private float _lerp;

        protected bool isArrived;
        protected int damage;
        protected string towerName;

        [SerializeField] private AnimationCurve curve;
        [SerializeField] private float speed;

        protected virtual void Awake()
        {
            _rigid = GetComponent<Rigidbody>();
            _meshRenderer = GetComponentInChildren<MeshRenderer>();
            _sphereCollider = GetComponent<SphereCollider>();
        }

        protected virtual void OnEnable()
        {
            _startPos = transform.position;
        }

        protected abstract void FixedUpdate();

        protected virtual void OnDisable()
        {
            isArrived = false;
            _lerp = 0;
            _meshRenderer.enabled = true;
            _sphereCollider.enabled = true;
        }

        protected virtual void OnTriggerEnter(Collider other)
        {
            isArrived = true;
            _meshRenderer.enabled = false;
            _sphereCollider.enabled = false;
            ProjectileHit(other);
        }

        protected void ParabolaPath(Vector3 endPos)
        {
            var gravity = _lerp < 0.5f ? 1f : 1.5f;
            _lerp += Time.deltaTime * gravity * speed;
            _curPos = Vector3.Lerp(_startPos, endPos, _lerp);
            _curPos.y += curve.Evaluate(_lerp);
            var rigidPos = _rigid.position;
            var dir = (_curPos - rigidPos).normalized;
            _rigid.position = _curPos;
            _rigid.MoveRotation(Quaternion.LookRotation(dir));
        }

        protected abstract void ProjectileHit(Collider col);

        protected void ApplyDamage(Collider col)
        {
            if (col.TryGetComponent(out IDamageable damageable))
            {
                damageable.Damage(damage);
                DataManager.SumDamage(towerName, damage);
            }
        }
    }
}