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
            if (!_archerUnit1) return;
            _archerUnit1.gameObject.SetActive(false);
            if (!_archerUnit2) return;
            _archerUnit2.gameObject.SetActive(false);
        }

        public override void Init(MeshFilter consMeshFilter)
        {
            base.Init(consMeshFilter);
            if (_archerUnit1) _archerUnit1.gameObject.SetActive(false);
            if (_archerUnit2) _archerUnit2.gameObject.SetActive(false);
        }

        public override void SetUp(MeshFilter towerMeshFilter, int unitHealth, float attackRange, float attackDelay)
        {
            base.SetUp(towerMeshFilter, unitHealth, attackRange, attackDelay);
            _archerUnit1 = StackObjectPool.Get<ArcherUnit>("ArcherUnit", archerPos[towerLevel].position);
            _archerUnit1.Init(atkDelay);
            _archerUnit1.GetComponent<Health>().InitializeHealth(health);
            if (towerLevel != 4) return;
            _archerUnit2 = StackObjectPool.Get<ArcherUnit>("ArcherUnit", archerPos[5].position);
            _archerUnit2.Init(atkDelay);
            _archerUnit2.GetComponent<Health>().InitializeHealth(health);
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