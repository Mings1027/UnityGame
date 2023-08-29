using DG.Tweening;
using GameControl;
using ManagerControl;
using UnityEngine;

namespace TowerControl
{
    public abstract class TargetingTower : Tower
    {
        private Collider[] _targetColliders;

        protected Transform target;
        protected int damage;
        protected bool isTargeting;

        public float TowerRange { get; private set; }

        [SerializeField] private LayerMask targetLayer;

        protected override void OnDisable()
        {
            base.OnDisable();
            CancelInvoke();
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawWireSphere(transform.position, TowerRange);
        }

        protected override void Init()
        {
            base.Init();
            _targetColliders = new Collider[3];
        }

        private void Targeting()
        {
            target = SearchTarget.ClosestTarget(transform.position, TowerRange, _targetColliders, targetLayer);
            isTargeting = target != null;

            if (!isTargeting) return;
            
            Attack();
        }

        protected abstract void Attack();

        public override void TowerSetting(MeshFilter towerMesh, int damageData, int rangeData,
            float attackDelayData)
        {
            base.TowerSetting(towerMesh, damageData, rangeData, attackDelayData);

            damage = damageData;
            TowerRange = rangeData;

            CancelInvoke();
            InvokeRepeating(nameof(Targeting), 1, attackDelayData);
        }
    }
}