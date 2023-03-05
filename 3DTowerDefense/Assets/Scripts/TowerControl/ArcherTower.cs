using GameControl;
using UnitControl;
using UnityEngine;

namespace TowerControl
{
    public class ArcherTower : Tower
    {
        private ArcherUnit[] _archerUnit;
        private Vector3 _targetDirection;

        [SerializeField] private Transform[] archerPos;

        protected override void Awake()
        {
            base.Awake();
            targetCount = 3;
            _archerUnit = new ArcherUnit[2];
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            ArcherUnitSetUp();
        }

        public override void Init(int unitHealth, int unitDamage, float attackRange, float attackDelay)
        {
            base.Init(unitHealth, unitDamage, attackRange, attackDelay);
            ArcherUnitSetUp();
        }

        public override void SetUp()
        {
            base.SetUp();
            var count = towerLevel == 4 ? 2 : 1;
            for (var i = 0; i < count; i++)
            {
                _archerUnit[i] = StackObjectPool.Get<ArcherUnit>("ArcherUnit", archerPos[towerLevel + i].position);
                _archerUnit[i].UnitSetup(damage, atkDelay);
            }
        }

        private void ArcherUnitSetUp()
        {
            foreach (var t in _archerUnit)
            {
                if (t != null && t.gameObject.activeSelf)
                {
                    t.gameObject.SetActive(false);
                }
            }
        }

        protected override void UpdateTarget()
        {
            var shortestDistance = Mathf.Infinity;
            Collider nearestEnemy = null;
            for (var i = 0; i < targetCount; i++)
            {
                if (targets[i] == null) continue;
                var distanceToEnemy = Vector3.Distance(transform.position, targets[i].transform.position);
                if (distanceToEnemy < shortestDistance)
                {
                    shortestDistance = distanceToEnemy;
                    nearestEnemy = targets[i];
                }
            }

            targets[0] = nearestEnemy != null && shortestDistance <= atkRange ? nearestEnemy : null;
            isTargeting = targets[0] != null;

            var count = towerLevel == 4 ? 2 : 1;
            for (var i = 0; i < count; i++)
            {
                if (!isTargeting) return;
                _archerUnit[i].IsTargeting = isTargeting;
                _archerUnit[i].Target = targets[0].transform;
            }
        }
    }
}