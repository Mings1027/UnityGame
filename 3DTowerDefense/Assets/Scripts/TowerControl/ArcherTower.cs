using GameControl;
using UnitControl;
using UnityEngine;

namespace TowerControl
{
    public class ArcherTower : Tower
    {
        [SerializeField] private Transform[] archerPos;
        private ArcherUnit _archerUnit1;
        private ArcherUnit _archerUnit2;
        private Vector3 _targetDirection;

        protected override void OnDisable()
        {
            base.OnDisable();
            if (!_archerUnit1) return;
            _archerUnit1.gameObject.SetActive(false);
            if (!_archerUnit2) return;
            _archerUnit2.gameObject.SetActive(false);
        }

        public override void SetUp(float attackRange, float attackDelay)
        {
            base.SetUp(attackRange, attackDelay);
            SpawnUnit();
        }

        private void SpawnUnit()
        {
            if (_archerUnit1) _archerUnit1.gameObject.SetActive(false);
            _archerUnit1 = StackObjectPool.Get<ArcherUnit>("ArcherUnit", archerPos[towerLevel].position);
            _archerUnit1.Setting(atkDelay);

            if (towerLevel != 4) return;
            if (_archerUnit2) _archerUnit2.gameObject.SetActive(false);
            _archerUnit2 = StackObjectPool.Get<ArcherUnit>("ArcherUnit", archerPos[towerLevel + 1].position);
            _archerUnit2.Setting(atkDelay);
        }
    }
}