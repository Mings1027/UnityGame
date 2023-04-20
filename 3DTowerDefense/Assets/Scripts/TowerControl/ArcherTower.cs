using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
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

        private Action onAttackEvent;

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

            onAttackEvent = null;
            onAttackEvent += TowerLevel != 4 ? SingleArcher : () => MultiArcher().Forget();
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
            onAttackEvent?.Invoke();
        }

        private void SingleArcher()
        {
            AmmoSpawn<Bullet>("Bullet", 0).Init(target, Damage);
        }

        private async UniTaskVoid MultiArcher()
        {
            for (var i = 0; i < 2; i++)
            {
                AmmoSpawn<Projectile>("ArrowProjectile", i).Init(target, Damage);

                await UniTask.Delay(500);
            }
        }

        private T AmmoSpawn<T>(string tagName, int index) where T : MonoBehaviour
        {
            _archerUnits[index].TargetUpdate(target, isTargeting);
            return StackObjectPool.Get<T>(tagName, _archerUnits[index].transform.position);
        }
    }
}