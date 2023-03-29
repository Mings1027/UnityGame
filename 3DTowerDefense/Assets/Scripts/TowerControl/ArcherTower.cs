using GameControl;
using UnitControl;
using UnityEngine;
using WeaponControl;

namespace TowerControl
{
    public class ArcherTower : TowerAttacker
    {
        private Vector3 _targetDirection;
        private ArcherUnit[] _archerUnits;

        [SerializeField] private Transform[] archerPos;

        protected override void Awake()
        {
            base.Awake();
            _archerUnits = new ArcherUnit[2];
            archerPos = new Transform[transform.GetChild(0).childCount];
            for (var i = 0; i < archerPos.Length; i++)
            {
                archerPos[i] = transform.GetChild(0).GetChild(i);
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            UnitDisable();
        }

        public override void ReadyToBuild(MeshFilter consMeshFilter)
        {
            base.ReadyToBuild(consMeshFilter);
            UnitDisable();
        }

        public override void Building(MeshFilter towerMeshFilter, int minDamage, int maxDamage, float range,
            float delay)
        {
            base.Building(towerMeshFilter, minDamage, maxDamage, range, delay);

            var count = TowerLevel == 4 ? 2 : 1;
            for (var i = 0; i < count; i++)
            {
                _archerUnits[i] = StackObjectPool.Get<ArcherUnit>("ArcherUnit", archerPos[TowerLevel + i].position);
            }
        }

        private void UnitDisable()
        {
            for (var i = 0; i < _archerUnits.Length; i++)
            {
                if (_archerUnits[i] == null || !_archerUnits[i].gameObject.activeSelf) continue;
                _archerUnits[i].gameObject.SetActive(false);
                _archerUnits[i] = null;
            }
        }

        protected override void Attack()
        {
            StackObjectPool.Get("ArrowShootSound", transform.position);
            if (TowerLevel != 4)
            {
                SingleArcher();
            }
            else
            {
                MultiArcher();
            }
        }

        private void SingleArcher()
        {
            var t = target.position + target.forward;
            SpawnArrow(_archerUnits[0].transform.position, t);
            _archerUnits[0].TargetUpdate(target, isTargeting);
        }

        private void MultiArcher()
        {
            for (var i = 0; i < 2; i++)
            {
                var t = target.position + target.forward;
                SpawnArrow(_archerUnits[i].transform.position, t);
                _archerUnits[i].TargetUpdate(target, isTargeting);
            }
        }

        private void SpawnArrow(Vector3 startPos, Vector3 endPos)
        {
            var p = StackObjectPool.Get<Projectile>("UnitArrow", startPos);
            p.Setting("Enemy", endPos, Damage);
        }
    }
}