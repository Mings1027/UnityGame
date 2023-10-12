using DataControl;
using InterfaceControl;
using ManagerControl;
using UnityEngine;

namespace ProjectileControl
{
    public abstract class Projectile : MonoBehaviour, IHit
    {
        private Collider _collider;
        private MeshRenderer projectileMesh;
        private ParticleSystem _trailParticle;
        private ParticleSystem _hitParticle;
        private Rigidbody _rigid;

        private ushort _damage;
        private float _gravity;
        private Vector3 _curPos;
        private Vector3 _startPos;
        private Vector3 _centerPos;
        private bool _isArrived;

        protected float lerp;
        protected Transform target;

        [SerializeField] private TargetingTowerData towerData;

        [SerializeField, Range(0, 50)] private float height;

        // [SerializeField] private AnimationCurve curve;
        [SerializeField] private float speed;

        protected virtual void Awake()
        {
            _collider = GetComponent<Collider>();
            projectileMesh = GetComponentInChildren<MeshRenderer>();
            _trailParticle = transform.GetChild(1).GetComponent<ParticleSystem>();
            _hitParticle = transform.GetChild(2).GetComponent<ParticleSystem>();
            _rigid = GetComponent<Rigidbody>();
        }

        protected virtual void OnEnable()
        {
            projectileMesh.enabled = true;
            _collider.enabled = true;
            _startPos = transform.position;
            _trailParticle.Play();
            lerp = 0;
            _isArrived = false;
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
            projectileMesh.enabled = false;
            _collider.enabled = false;
            Hit();
        }

        /*============================================================================================================
         *                                  Unity Event
         ============================================================================================================*/

        protected void ProjectilePath(Vector3 endPos)
        {
            if (_isArrived) return;
            _gravity = Mathf.Lerp(0.8f, 1.5f, lerp);
            lerp += Time.deltaTime * _gravity * speed;
            _centerPos = (_startPos + endPos) * 0.5f + Vector3.up * height;
            _curPos = Vector3.Lerp(
                Vector3.Lerp(_startPos, _centerPos, lerp),
                Vector3.Lerp(_centerPos, endPos, lerp),
                lerp);
            var rigidPos = _rigid.position;
            var dir = (_curPos - rigidPos).normalized;
            _rigid.position = _curPos;
            _rigid.MoveRotation(Quaternion.LookRotation(dir));
        }

        public virtual void Init(ushort dmg, Transform t)
        {
            _damage = dmg;
            target = t;
        }

        public virtual void ColorInit(sbyte effectIndex)
        {
            var trailColor = _trailParticle.main;
            trailColor.startColor = towerData.ProjectileColor[effectIndex];
            var hitColor = _hitParticle.main;
            hitColor.startColor = towerData.ProjectileColor[effectIndex];
        }

        protected void TryDamage(Transform t)
        {
            if (!t.TryGetComponent(out IDamageable damageable) || !t.gameObject.activeSelf) return;
            damageable.Damage(_damage);
            DataManager.SumDamage(towerData.TowerType, _damage);
        }

        public virtual void Hit()
        {
            TryDamage(target);
        }
    }
}