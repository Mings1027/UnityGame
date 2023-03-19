using AttackControl;
using UnityEngine;

namespace TowerControl
{
    public class MageTower : Tower
    {
        private TargetFinder _targetFinder;
        private Transform _target;
        private bool _isTargeting;

        protected override void Awake()
        {
            base.Awake();
            _targetFinder = GetComponent<TargetFinder>();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            InvokeRepeating(nameof(FindingTarget), 0, 1f);
        }

        public override void Building(MeshFilter towerMeshFilter, int minDamage, int maxDamage, float range,
            float delay, int health = 0)
        {
            base.Building(towerMeshFilter, minDamage, maxDamage, range, delay, health);
            GetComponent<TargetFinder>().SetUp(delay, range, minDamage, maxDamage);
        }

        private void FindingTarget()
        {
            var t = _targetFinder.FindClosestTarget();
            _target = t.Item1;
            _isTargeting = t.Item2;
        }
    }
}