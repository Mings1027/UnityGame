using AttackControl;
using GameControl;
using UnitControl;
using UnityEngine;

namespace TowerControl
{
    public class ArcherTower : TowerUnitAttacker
    {
        private Vector3 _targetDirection;
        private ArcherUnit[] _archerUnits;

        [SerializeField] private Transform[] archerPos;

        protected override void Awake()
        {
            base.Awake();
            _archerUnits = new ArcherUnit[2];
        }

        public override void ReadyToBuild(MeshFilter consMeshFilter)
        {
            base.ReadyToBuild(consMeshFilter);
            UnitSetUp();
        }

        protected override void UnitSetUp()
        {
            foreach (var t in _archerUnits)
            {
                if (t != null && t.gameObject.activeSelf)
                {
                    t.gameObject.SetActive(false);
                }
            }
        }

        protected override void UnitUpgrade(int minDamage, int maxDamage, float range, float delay, int health = 0)
        {
            var count = TowerLevel == 4 ? 2 : 1;
            for (var i = 0; i < count; i++)
            {
                _archerUnits[i] = StackObjectPool.Get<ArcherUnit>("ArcherUnit", archerPos[TowerLevel + i].position);
                _archerUnits[i].GetComponent<TargetFinder>().SetUp(minDamage, maxDamage, range, delay);
            }
        }
    }
}