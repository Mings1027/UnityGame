using AttackControl;
using DG.Tweening;
using UnityEngine;

namespace TowerControl
{
    public abstract class TargetingTower : Tower
    {
        private Collider[] _targetColliders;

        private Tween _delayTween;


        private bool attackAble;

        protected Transform target;
        protected bool isTargeting;

        protected abstract void Attack();

        protected override void Awake()
        {
            base.Awake();
            _targetColliders = new Collider[5];
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            attackAble = true;
            InvokeRepeating(nameof(FindingTarget), 1f, 1f);
        }

        private void Update()
        {
            if (isUpgrading || !attackAble || !isTargeting) return;
            Attack();
            StartCoolDown();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            CancelInvoke();
            _delayTween?.Kill();
        }

        protected override void FindingTarget()
        {
            var c = SearchTarget.ClosestTarget(transform.position, atkRange, _targetColliders, TargetLayer);
            target = c.Item1;
            isTargeting = c.Item2;
        }

        private void StartCoolDown()
        {
            attackAble = false;
            _delayTween.Restart();
        }

        public override void TowerInit(MeshFilter consMeshFilter)
        {
            base.TowerInit(consMeshFilter);
            isUpgrading = true;
        }

        public override void TowerSetting(MeshFilter towerMeshFilter, int minDamage, int maxDamage, float range,
            float delay)
        {
            base.TowerSetting(towerMeshFilter, minDamage, maxDamage, range, delay);

            _delayTween?.Kill();
            _delayTween = DOVirtual.DelayedCall(delay, () => attackAble = true, false).SetAutoKill(false);
        }
    }
}