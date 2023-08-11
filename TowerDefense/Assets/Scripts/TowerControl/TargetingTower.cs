using System;
using DG.Tweening;
using GameControl;
using UnityEngine;

namespace TowerControl
{
    public abstract class TargetingTower : Tower
    {
        private Tween _delayTween;
        private bool _attackAble;
        public float TowerRange { get; private set; }
        protected Transform target;
        protected int damage;
        protected bool isTargeting;

        protected Action onAttackEvent;
        protected Collider[] targetColliders;

        [SerializeField] private LayerMask targetLayer;

        protected override void Awake()
        {
            base.Awake();
            onAttackEvent = () => { };
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            _attackAble = true;
            InvokeRepeating(nameof(Targeting), 1, 0.5f);
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
            onAttackEvent = null;
            CancelInvoke();
            _delayTween?.Kill();
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawWireSphere(transform.position, TowerRange);
        }

        private void Targeting()
        {
            target = SearchTarget.ClosestTarget(transform.position, TowerRange, targetColliders, targetLayer);
            isTargeting = target != null;
        }

        protected virtual void Attack()
        {
            onAttackEvent.Invoke();
        }

        private void StartCoolDown()
        {
            _attackAble = false;
            _delayTween.Restart();
        }

        public override void TowerSetting(MeshFilter towerMesh, int damageData, int attackRangeData,
            float attackDelayData)
        {
            base.TowerSetting(towerMesh, damageData, attackRangeData, attackDelayData);

            damage = damageData;
            TowerRange = attackRangeData;
            _delayTween?.Kill();
            _delayTween = DOVirtual.DelayedCall(attackDelayData, () => _attackAble = true, false).SetAutoKill(false);
        }
    }
}