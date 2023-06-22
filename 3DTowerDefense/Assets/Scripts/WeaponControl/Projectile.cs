using System;
using GameControl;
using UnityEngine;

namespace WeaponControl
{
    public abstract class Projectile : MonoBehaviour
    {
        protected Rigidbody rigid;
        protected int damage;
        protected float lerp;
        protected Vector3 curPos;
        protected Vector3 startPos;
        protected float ProjectileSpeed => speed;
        protected AnimationCurve ProjectileCurve => curve;

        [SerializeField] private AnimationCurve curve;
        [SerializeField] private float speed;

        protected virtual void Awake()
        {
            rigid = GetComponent<Rigidbody>();
        }

        protected virtual void OnEnable()
        {
            lerp = 0;
            startPos = transform.position;
        }

        protected virtual void FixedUpdate()
        {
            ParabolaPath();
        }

        protected abstract void OnTriggerEnter(Collider other);

        protected abstract void ParabolaPath();

        protected abstract void ProjectileHit(Collider col);

        protected void Damaging(Collider col)
        {
            if (col.TryGetComponent(out Health h))
            {
                h.TakeDamage(damage);
            }
        }

        // public void Init(Transform target, int damage)
        // {
        //     _target = target;
        //     _damage = damage;
        // }
    }
}