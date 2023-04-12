using System;
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

        protected int level;

        protected float projSpeed
        {
            get => speed;
            set => speed = value;
        }

        [SerializeField] private string tagName;
        [SerializeField] private AnimationCurve curve;
        [SerializeField] private float speed;

        protected virtual void OnEnable()
        {
            lerp = 0;
            startPos = transform.position;
        }

        protected abstract void FixedUpdate();

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag(tagName))
            {
                ProjectileHit(other);
            }

            if (other.CompareTag("Ground")) gameObject.SetActive(false);
        }

        protected void ParabolaPath()
        {
            var gravity = lerp < 0.5f ? 1f : 1.2f;
            lerp += Time.fixedDeltaTime * gravity * projSpeed;
            curPos = Vector3.Lerp(startPos, target.position, lerp);
            curPos.y += curve.Evaluate(lerp);
            var t = transform;
            var dir = (curPos - t.position).normalized;
            t.position = curPos;
            t.forward = dir;
        }

        protected void StraightPath()
        {
            lerp += Time.fixedDeltaTime * projSpeed;
            curPos = Vector3.Lerp(startPos, target.position, lerp);
            var t = transform;
            var dir = (curPos - t.position).normalized;
            t.position = curPos;
            t.forward = dir;
        }

        protected virtual void ProjectileHit(Collider col)
        {
            if (col.TryGetComponent(out Health h))
            {
                h.TakeDamage(damage, col.gameObject);
            }

            gameObject.SetActive(false);
        }

        public void Init(Transform t, int d)
        {
            target = t;
            damage = d;
        }

        public virtual void Init(Transform t, int d, int towerLevel)
        {
            target = t;
            damage = d;
            level = towerLevel;
        }
    }
}