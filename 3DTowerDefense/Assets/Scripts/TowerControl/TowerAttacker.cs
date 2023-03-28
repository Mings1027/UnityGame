using AttackControl;
using UnityEngine;

namespace TowerControl
{
    public abstract class TowerAttacker : Tower
    {
        protected TargetFinder targetFinder;

        protected abstract void Attack();

        protected override void Awake()
        {
            base.Awake();
            targetFinder = GetComponent<TargetFinder>();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            CancelInvoke();
        }

        private void Update()
        {
            if (!isUpgrading && targetFinder.attackAble && targetFinder.IsTargeting)
            {
                Attack();
                targetFinder.StartCoolDown();
            }
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
            GetComponent<TargetFinder>().SetUp(minDamage, maxDamage, range, delay);
        }
    }
}