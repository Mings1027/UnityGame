using GameControl;
using UnitControl;
using Unity.Mathematics;
using UnityEngine;
using WeaponControl;

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
            _archerUnit1.Init(atkDelay);
            if (towerLevel != 4) return;
            if (_archerUnit2) _archerUnit2.gameObject.SetActive(false);
            _archerUnit2 = StackObjectPool.Get<ArcherUnit>("ArcherUnit", archerPos[5].position);
            _archerUnit2.Init(atkDelay);
        }

        protected override void Attack()
        {
            if (!isTargeting) return;
            var targetPos = target.position + target.forward;

            _archerUnit1.UpdateTarget(isTargeting, isTargeting ? targetPos : transform.position);

            if (!_archerUnit2) return;

            _archerUnit2.UpdateTarget(isTargeting, isTargeting ? targetPos : transform.position);
        }
    }
}