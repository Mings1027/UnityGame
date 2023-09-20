using InterfaceControl;
using ManagerControl;
using UnityEngine;

namespace ProjectileControl
{
    public abstract class Projectile : MonoBehaviour
    {
        private Collider _collider;
        private ParticleSystem _trailParticle;
        private ParticleSystem _hitParticle;
        private Rigidbody _rigid;
        private Vector3 _curPos;
        private Vector3 _startPos;
        private int _damage;
        private float _lerp;
        private bool _isArrived;
        private TowerType _towerType;

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
            _trailParticle.Play();
            _startPos = transform.position;
        }

        protected virtual void FixedUpdate()
        {
            ProjectilePath(target.position);
            if (_isArrived && _hitParticle.isStopped)
            {
                gameObject.SetActive(false);
            }
        }

        protected virtual void OnTriggerEnter(Collider other)
        {
            _isArrived = true;
            _hitParticle.Play();
            _collider.enabled = false;
            TryHit();
        }

        protected virtual void OnDisable()
        {
            _lerp = 0;
            _isArrived = false;
        }

        protected void ProjectilePath(Vector3 endPos)
        {
            if (_isArrived) return;
            var gravity = _lerp < 0.5f ? 1f : 1.5f;
            _lerp += Time.deltaTime * gravity * speed;
            _curPos = Vector3.Lerp(_startPos, endPos, _lerp);
            _curPos.y += curve.Evaluate(_lerp);
            var rigidPos = _rigid.position;
            var dir = (_curPos - rigidPos).normalized;
            _rigid.position = _curPos;
            _rigid.MoveRotation(Quaternion.LookRotation(dir));
        }

        protected virtual void TryHit()
        {
            TryDamage(target);
        }

        public virtual void Init(int dmg, Transform t, TowerType towerType)
        {
            _damage = dmg;
            target = t;
            _towerType = towerType;
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
            DataManager.SumDamage(ref _towerType, _damage);
        }
    }
}