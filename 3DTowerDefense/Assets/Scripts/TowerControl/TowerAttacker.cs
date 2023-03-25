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
                targetFinder.StartCoolDown().Forget();
            }
        }

        public override void ReadyToBuild(MeshFilter consMeshFilter)
        {
            base.ReadyToBuild(consMeshFilter);
            isUpgrading = true;
        }

        public override void Building(MeshFilter towerMeshFilter, int minDamage, int maxDamage, float range,
            float delay, int health = 0)
        {
            base.Building(towerMeshFilter, minDamage, maxDamage, range, delay, health);
            GetComponent<TargetFinder>().SetUp(minDamage, maxDamage, range, delay);
            isUpgrading = false;
        }
    }
}