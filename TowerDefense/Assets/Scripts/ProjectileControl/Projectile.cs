using System;
using InterfaceControl;
using ManagerControl;
using UnityEngine;

namespace ProjectileControl
{
    public abstract class Projectile : MonoBehaviour, IHit
    {
        private Collider _collider;
        private ParticleSystem _trailParticle;
        private ParticleSystem _hitParticle;
        private Rigidbody _rigid;

        private int _damage;
        private float _gravity;
        private Vector3 _curPos;
        private Vector3 _startPos;
        private bool _isArrived;

        protected TowerType towerType;
        protected float lerp;
        protected Transform target;

        [SerializeField] private AnimationCurve curve;
        [SerializeField] private float speed;

        protected virtual void Awake()
        {
            _collider = GetComponent<Collider>();
            _trailParticle = transform.GetChild(0).GetComponent<ParticleSystem>();
            _hitParticle = transform.GetChild(1).GetComponent<ParticleSystem>();
            _rigid = GetComponent<Rigidbody>();
        }

        protected virtual void OnEnable()
        {
            _collider.enabled = true;
            _startPos = transform.position;
            _trailParticle.Play();
        }

        protected virtual void FixedUpdate()
        {
            ProjectilePath(target.position);
        }

        private void LateUpdate()
        {
            if (_isArrived && _hitParticle.isStopped)
            {
                gameObject.SetActive(false);
            }
        }

        protected virtual void OnTriggerEnter(Collider other)
        {
            _hitParticle.Play();
            _isArrived = true;
            _collider.enabled = false;
            Hit();
        }

        protected virtual void OnDisable()
        {
            lerp = 0;
            _isArrived = false;
        }

        /*============================================================================================================
         *                                  Unity Event
         ============================================================================================================*/

        protected void ProjectilePath(Vector3 endPos)
        {
            if (_isArrived) return;
            _gravity = lerp < 0.5f ? 1 : 1.5f;
            lerp += Time.deltaTime * _gravity * speed;
            _curPos = Vector3.Lerp(_startPos, endPos, lerp);
            _curPos.y += curve.Evaluate(lerp);
            var rigidPos = _rigid.position;
            var dir = (_curPos - rigidPos).normalized;
            _rigid.position = _curPos;
            _rigid.MoveRotation(Quaternion.LookRotation(dir));
        }

        public virtual void Init(int dmg, Transform t)
        {
            _damage = dmg;
            target = t;
        }

        public void ColorInit(ref ParticleSystem.MinMaxGradient minMaxGradient)
        {
            var color = _trailParticle.colorOverLifetime;
            color.color = new ParticleSystem.MinMaxGradient(minMaxGradient.colorMin, minMaxGradient.colorMax);
        }

        protected void TryDamage(Transform t)
        {
            t.TryGetComponent(out IDamageable damageable);
            damageable.Damage(_damage);
            DataManager.SumDamage(towerType, _damage);
        }

        public virtual void Hit()
        {
            TryDamage(target);
        }
    }
}