using GameControl;
using UnitControl;
using UnityEngine;

namespace TowerControl
{
    public class ArcherTower : Tower
    {
        private Unit _archerUnit1;
        private Unit _archerUnit2;
        private Vector3 _targetDirection;

        [SerializeField] private Transform[] archerPos;

        protected override void OnDisable()
        {
            base.OnDisable();
            SetUp();
        }

        public override void SetUp()
        {
            if (_archerUnit1) _archerUnit1.gameObject.SetActive(false);
            if (_archerUnit2) _archerUnit2.gameObject.SetActive(false);
        }

        public override void Init(int unitHealth, int unitDamage, float attackRange, float attackDelay)
        {
            base.Init(unitHealth, unitDamage, attackRange, attackDelay);
            _archerUnit1 = StackObjectPool.Get<ArcherUnit>("ArcherUnit", archerPos[towerLevel].position);
            _archerUnit1.Init(damage, atkDelay);
            if (towerLevel != 4) return;
            _archerUnit2 = StackObjectPool.Get<ArcherUnit>("ArcherUnit", archerPos[5].position);
            _archerUnit2.Init(damage, atkDelay);
        }

        protected override void Targeting()
        {
            _archerUnit1.IsTargeting = isTargeting;
            _archerUnit1.Target = target;
            if (!_archerUnit2) return;
            _archerUnit2.IsTargeting = isTargeting;
            _archerUnit2.Target = target;
        }
    }
}