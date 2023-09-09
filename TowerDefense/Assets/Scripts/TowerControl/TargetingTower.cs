using GameControl;
using ProjectileControl;
using UnityEngine;

namespace TowerControl
{
    public abstract class TargetingTower : Tower
    {
        private AudioSource _audioSource;
        private Collider[] _targetColliders;
        private int _effectIndex;
        private bool _isAttack;
        private float _atkDelay;
        private float _nextFireTime;

        protected Transform target;
        protected int damage;
        protected bool isTargeting;
        protected string[] effectName;

        public float TowerRange { get; private set; }

        [SerializeField] private LayerMask targetLayer;
        [SerializeField] private float repeatTime;

        protected override void Awake()
        {
            base.Awake();
            _audioSource = GetComponent<AudioSource>();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            _effectIndex = -1;
            InvokeRepeating(nameof(Targeting), 1, repeatTime);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            CancelInvoke();
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawWireSphere(transform.position, TowerRange);
        }

        private void FixedUpdate()
        {
            if (!isTargeting) return;
            if (Time.time < _nextFireTime) return;
            Attack();
            StartCooldown();
            _audioSource.PlayOneShot(_audioSource.clip);
        }

        protected override void Init()
        {
            base.Init();
            _targetColliders = new Collider[3];
        }

        private void Targeting()
        {
            var size = Physics.OverlapSphereNonAlloc(transform.position, TowerRange, _targetColliders, targetLayer);
            if (size <= 0)
            {
                target = null;
                isTargeting = false;
                return;
            }

            var shortestDistance = Mathf.Infinity;
            Transform nearestTarget = null;
            for (var i = 0; i < size; i++)
            {
                var distanceToResult =
                    Vector3.SqrMagnitude(transform.position - _targetColliders[i].transform.position);
                if (distanceToResult >= shortestDistance) continue;
                shortestDistance = distanceToResult;
                nearestTarget = _targetColliders[i].transform;
            }

            target = nearestTarget;
            isTargeting = true;
        }

        private void StartCooldown() => _nextFireTime = Time.time + _atkDelay;

        protected abstract void Attack();

        protected void EffectAttack(Transform t)
        {
            ObjectPoolManager.Get<FollowProjectile>(effectName[_effectIndex], t).target = t;
        }

        public override void TowerSetting(MeshFilter towerMesh, int damageData, int rangeData,
            float attackDelayData)
        {
            base.TowerSetting(towerMesh, damageData, rangeData, attackDelayData);

            damage = damageData;
            TowerRange = rangeData;
            _atkDelay = attackDelayData;

            if (TowerLevel % 2 == 0)
            {
                _effectIndex++;
            }

            // CancelInvoke();
            // InvokeRepeating(nameof(Targeting), 1, attackDelayData);
        }
    }
}