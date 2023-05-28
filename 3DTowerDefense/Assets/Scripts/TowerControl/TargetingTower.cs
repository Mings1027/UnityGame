using DG.Tweening;
using GameControl;
using UnityEngine;

namespace TowerControl
{
    public abstract class TargetingTower : Tower
    {
        private Tween _delayTween;
        private bool _attackAble;

        protected Transform target;
        protected bool isTargeting;

        protected abstract void Attack();

        protected override void Awake()
        {
            base.Awake();
            targetColliders = new Collider[5];
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            _attackAble = true;
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
            _delayTween?.Kill();
        }

        protected override void Targeting()
        {
            target = SearchTarget.ClosestTarget(transform.position, atkRange, targetColliders, TargetLayer);
            isTargeting = target != null;
        }

        private void StartCoolDown()
        {
            _attackAble = false;
            _delayTween.Restart();
        }

        public override void TowerInit(MeshFilter consMeshFilter)
        {
            base.TowerInit(consMeshFilter);
            isUpgrading = true;
        }

        public override void TowerSetting(MeshFilter towerMeshFilter, int minDamage, int maxDamage, float range,
            float delay, float health = 0)
        {
            base.TowerSetting(towerMeshFilter, minDamage, maxDamage, range, delay, health);
            _delayTween?.Kill();
            _delayTween = DOVirtual.DelayedCall(delay, () => _attackAble = true, false).SetAutoKill(false);
        }
    }
}