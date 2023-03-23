using AttackControl;
using UnityEngine;

namespace TowerControl
{
    public abstract class TowerAttacker : Tower
    {
        protected TargetFinder targetFinder;

        protected abstract void CheckState();
        protected abstract void Attack();

        protected override void Awake()
        {
            base.Awake();
            targetFinder = GetComponent<TargetFinder>();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            InvokeRepeating(nameof(CheckState), 0, 0.1f);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            CancelInvoke();
        }

        public override void Building(MeshFilter towerMeshFilter, int minDamage, int maxDamage, float range,
            float delay, int health = 0)
        {
            base.Building(towerMeshFilter, minDamage, maxDamage, range, delay, health);
            GetComponent<TargetFinder>().SetUp(minDamage, maxDamage, range, delay);
        }
    }
}