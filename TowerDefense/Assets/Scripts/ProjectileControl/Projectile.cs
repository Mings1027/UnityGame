using DataControl;
using InterfaceControl;
using ManagerControl;
using UnityEngine;

namespace ProjectileControl
{
    public abstract class Projectile : MonoBehaviour, IHit
    {
        private ParticleSystem _trailParticle;
        private ParticleSystem _hitParticle;
        private MeshRenderer _projectileMesh;
        private ProjectileDamageSource _projectileDamageSource;

        private int _damage;
        private float _gravity;
        private Vector3 _curPos;
        private Vector3 _startPos;
        private Vector3 _centerPos;
        private bool _isArrived;

        protected float lerp;

        public Collider target { get; private set; }

        [SerializeField] private TargetingTowerData towerData;
        [SerializeField, Range(0, 50)] private float height;
        [SerializeField] private float speed;

        protected virtual void Awake()
        {
            _projectileMesh = transform.GetChild(0).GetComponent<MeshRenderer>();
            _projectileDamageSource = _projectileMesh.GetComponent<ProjectileDamageSource>();
            _trailParticle = transform.GetChild(1).GetComponent<ParticleSystem>();
            _hitParticle = transform.GetChild(2).GetComponent<ParticleSystem>();
        }

        protected virtual void OnEnable()
        {
            _projectileMesh.enabled = true;
            _projectileDamageSource.enabled = true;
            _startPos = transform.position;
            _trailParticle.Play();
            lerp = 0;
            _isArrived = false;
        }

        protected virtual void Update()
        {
            ProjectilePath(target.bounds.center);
        }

        private void LateUpdate()
        {
            if (_isArrived && _trailParticle.isStopped)
            {
                gameObject.SetActive(false);
            }
        }

        /*============================================================================================================
         *                                  Unity Event
         ============================================================================================================*/

        protected void ProjectilePath(Vector3 endPos)
        {
            if (lerp < 1)
            {
                _gravity = Mathf.Lerp(0.8f, 1.5f, lerp);
                lerp += Time.deltaTime * _gravity * speed;
                _centerPos = (_startPos + endPos) * 0.5f + Vector3.up * height;
                _curPos = Vector3.Lerp(Vector3.Lerp(_startPos, _centerPos, lerp),
                    Vector3.Lerp(_centerPos, endPos, lerp), lerp);
                var t = transform;
                var dir = (_curPos - t.position).normalized;
                t.position = _curPos;
                _projectileDamageSource.transform.forward = dir;
            }
            else
            {
                _projectileMesh.enabled = false;
                _projectileDamageSource.enabled = false;
            }
        }

        public virtual void Init(int dmg, Collider t)
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

        public virtual void Hit()
        {
            _hitParticle.Play();
            _isArrived = true;
        }

        protected void TryDamage(Collider t)
        {
            if (!t.TryGetComponent(out IDamageable damageable) || !t.enabled) return;
            damageable.Damage(_damage);
        }
    }
}