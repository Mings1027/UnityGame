using AttackControl;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TowerControl
{
    public abstract class TowerAttacker : Tower
    {
        private Collider[] _targetColliders;
        private Tween _delayTween;
        private int _minDamage, _maxDamage;
        private float _atkRange;
        private bool _attackAble;

        protected int Damage => Random.Range(_minDamage, _maxDamage);
        protected Transform target;
        protected bool isTargeting;

        [SerializeField] private LayerMask targetLayer;

        protected abstract void Attack();

        protected override void Awake()
        {
            base.Awake();
            _targetColliders = new Collider[5];
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            _attackAble = true;
            InvokeRepeating(nameof(FindTarget), 1f, 1f);
        }

        private void Update()
        {
            if (isUpgrading || !_attackAble || !isTargeting) return;
            Attack();
            StartCoolDown();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            CancelInvoke();
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, _atkRange);
        }

        private void FindTarget()
        {
            var c = SearchTarget.ClosestTarget(transform.position, _atkRange, _targetColliders, targetLayer);
            target = c.Item1;
            isTargeting = c.Item2;
        }

        private void StartCoolDown()
        {
            _attackAble = false;
            _delayTween.Restart();
        }

        public override void ReadyToBuild(MeshFilter consMeshFilter)
        {
            base.ReadyToBuild(consMeshFilter);
            isUpgrading = true;
        }

        public override void Building(MeshFilter towerMeshFilter, int minDamage, int maxDamage, float range,
            float delay)
        {
            base.Building(towerMeshFilter, minDamage, maxDamage, range, delay);
            _minDamage = minDamage;
            _maxDamage = maxDamage;
            _atkRange = range;
            _delayTween?.Kill();
            _delayTween = DOVirtual.DelayedCall(delay, () => _attackAble = true).SetAutoKill(false);
        }
    }
}