using GameControl;
using UnitControl;
using UnityEngine;

namespace TowerControl
{
    public class ArcherTower : Tower
    {
        private ArcherUnit[] _archerUnits;
        private Vector3 _targetDirection;

        [SerializeField] private Transform[] archerPos;

        protected override void Awake()
        {
            base.Awake();
            targetCount = 3;
            _archerUnits = new ArcherUnit[2];
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            ArcherUnitSetUp();
        }

        public override void Init(int unitHealth, int unitDamage, float attackDelay, float attackRange)
        {
            base.Init(unitHealth, unitDamage, attackDelay, attackRange);
            ArcherUnitSetUp();
        }

        public override void SetUp()
        {
            base.SetUp();
            var count = towerLevel == 4 ? 2 : 1;
            for (var i = 0; i < count; i++)
            {
                _archerUnits[i] = StackObjectPool.Get<ArcherUnit>("ArcherUnit", archerPos[towerLevel + i].position);
                _archerUnits[i].UnitSetup(damage, atkDelay, atkRange);
            }
        }

        private void ArcherUnitSetUp()
        {
            foreach (var t in _archerUnits)
            {
                if (t != null && t.gameObject.activeSelf)
                {
                    t.gameObject.SetActive(false);
                }
            }
        }

        protected override void UpdateTarget()
        {
        }
    }
}