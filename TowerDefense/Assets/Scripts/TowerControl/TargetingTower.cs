using System;
using DG.Tweening;
using GameControl;
using ManagerControl;
using ProjectileControl;
using UnityEngine;

namespace TowerControl
{
    public abstract class TargetingTower : Tower
    {
        private Collider[] _targetColliders;
        private int _effectCount;

        protected Transform target;
        protected int damage;
        protected bool isTargeting;
        protected string[] effectName;

        public float TowerRange { get; private set; }

        [SerializeField] private LayerMask targetLayer;

        protected override void OnDisable()
        {
            base.OnDisable();
            CancelInvoke();
            _effectCount = 0;
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

        protected void EffectAttack(Transform t)
        {
            for (int i = 0; i < _effectCount; i++)
            {
                ObjectPoolManager.Get<FollowProjectile>(effectName[i], t).target = t;
            }
        }

        public override void TowerSetting(MeshFilter towerMesh, int damageData, int rangeData,
            float attackDelayData)
        {
            base.TowerSetting(towerMesh, damageData, rangeData, attackDelayData);

            damage = damageData;
            TowerRange = rangeData;

            if (TowerLevel % 2 == 0)
            {
                _effectCount++;
            }

            CancelInvoke();
            InvokeRepeating(nameof(Targeting), 1, attackDelayData);
        }
    }
}