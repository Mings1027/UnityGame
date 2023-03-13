using AttackControl;
using GameControl;
using UnitControl;
using UnityEngine;

namespace TowerControl
{
    public class ArcherTower : Tower
    {
        private Vector3 _targetDirection;
        private ArcherUnit[] _archerUnits;

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

        public override void ReadyToBuild(MeshFilter consMeshFilter)
        {
            base.ReadyToBuild(consMeshFilter);
            ArcherUnitSetUp();
        }

        public override void Building( bool haveUnit,MeshFilter towerMeshFilter, float delay, float range, int damage,
            int health = 0)
        {
            base.Building(haveUnit,towerMeshFilter,  delay, range, damage, health);
            var count = towerLevel == 4 ? 2 : 1;
            for (var i = 0; i < count; i++)
            {
                _archerUnits[i] = StackObjectPool.Get<ArcherUnit>("ArcherUnit", archerPos[towerLevel + i].position);
                _archerUnits[i].GetComponent<TargetFinder>().SetUp(delay, damage, range);
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

        // protected override void UnitControl()
        // {
        //     var count = towerLevel == 4 ? 2 : 1;
        //     for (var i = 0; i < count; i++)
        //     {
        //         _archerUnits[i].target = target;
        //         _archerUnits[i].Attack();
        //     }
        // }
    }
}