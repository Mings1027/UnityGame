using DG.Tweening;
using GameControl;
using UnityEngine;

namespace TowerControl
{
    public abstract class TargetingTower : Tower
    {
        private Tween _delayTween;
        private Collider[] targetColliders;
        private bool _isAttack;

        protected Transform target;
        protected int damage;
        protected bool isTargeting;

        public float TowerRange { get; private set; }

        [SerializeField] private LayerMask targetLayer;

        protected override void OnEnable()
        {
            base.OnEnable();
            InvokeRepeating(nameof(Targeting), 1, 0.5f);
        }

        private void FixedUpdate()
        {
            if (_isAttack || !isTargeting) return;
            Attack();
            StartCoolDown();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            CancelInvoke();
        }

        private void OnDestroy()
        {
            _delayTween?.Kill();
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawWireSphere(transform.position, TowerRange);
        }

        protected override void Init()
        {
            base.Init();
            targetColliders = new Collider[3];
        }

        private void Targeting()
        {
            target = SearchTarget.ClosestTarget(transform.position, TowerRange, targetColliders, targetLayer);
            isTargeting = target != null;
        }

        protected abstract void Attack();

        private void StartCoolDown()
        {
            _isAttack = true;
            _delayTween.Restart();
        }

        public override void TowerSetting(MeshFilter towerMesh, int damageData, int attackRangeData,
            float attackDelayData)
        {
            base.TowerSetting(towerMesh, damageData, attackRangeData, attackDelayData);

            damage = damageData;
            TowerRange = attackRangeData;
            _delayTween = DOVirtual.DelayedCall(attackDelayData, () => _isAttack = false, false).SetAutoKill(false);
        }
    }
}