using AttackControl;
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
            _archerUnits = new ArcherUnit[2];
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            ArcherUnitSetUp();
        }

        public override void UnitInit()
        {
            base.UnitInit();
            ArcherUnitSetUp();
        }

        public override void SetUp(float attackDelay, int unitDamage, int unitHealth)
        {
            base.SetUp(attackDelay, unitDamage, unitHealth);
            var count = towerLevel == 4 ? 2 : 1;
            for (var i = 0; i < count; i++)
            {
                _archerUnits[i] = StackObjectPool.Get<ArcherUnit>("ArcherUnit", archerPos[towerLevel + i].position);
                _archerUnits[i].UnitSetUp(damage);
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

        protected override void UnitControl()
        {
            var count = towerLevel == 4 ? 2 : 1;
            for (var i = 0; i < count; i++)
            {
                _archerUnits[i].target = target;
                _archerUnits[i].Attack();
            }
        }
    }
}