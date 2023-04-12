using Cysharp.Threading.Tasks;
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
        private Transform[] _archerPos;

        protected override void Awake()
        {
            base.Awake();
            _archerUnits = new ArcherUnit[2];
            
            var archerPosition = GameObject.Find("ArcherPosition").transform;
            _archerPos = new Transform[archerPosition.childCount];
            for (var i = 0; i < _archerPos.Length; i++)
            {
                _archerPos[i] = archerPosition.GetChild(i);
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            UnitDisable();
        }

        public override void UnderConstruction(MeshFilter consMeshFilter)
        {
            base.UnderConstruction(consMeshFilter);
            UnitDisable();
        }

        public override void ConstructionFinished(MeshFilter towerMeshFilter, int minDamage, int maxDamage, float range,
            float delay)
        {
            base.ConstructionFinished(towerMeshFilter, minDamage, maxDamage, range, delay);

            var count = TowerLevel == 4 ? 2 : 1;
            for (var i = 0; i < count; i++)
            {
                _archerUnits[i] = StackObjectPool.Get<ArcherUnit>("ArcherUnit", _archerPos[TowerLevel + i].position);
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
            StackObjectPool.Get("ArrowShootSFX", transform.position);
            if (TowerLevel != 4)
            {
                SingleArcher();
            }
            else
            {
                MultiArcher().Forget();
            }
        }

        private void SingleArcher()
        {
            StackObjectPool.Get<Projectile>("ArrowBullet", _archerUnits[0].transform.position)
                .Init(target, Damage, TowerLevel);
            _archerUnits[0].TargetUpdate(target, isTargeting);
        }

        private async UniTaskVoid MultiArcher()
        {
            for (var i = 0; i < 2; i++)
            {
                StackObjectPool.Get<Projectile>("ArrowBullet", _archerUnits[i].transform.position)
                    .Init(target, Damage, TowerLevel);
                _archerUnits[i].TargetUpdate(target, isTargeting);

                await UniTask.Delay(100);
            }
        }
    }
}