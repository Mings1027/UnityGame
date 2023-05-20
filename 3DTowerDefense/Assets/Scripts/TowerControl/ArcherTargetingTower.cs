using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using GameControl;
using UnitControl;
using UnityEngine;
using WeaponControl;

namespace TowerControl
{
    public class ArcherTargetingTower : TargetingTower
    {
        private int archerCount;
        private Vector3 _targetDirection;
        private ArcherUnit[] _archerUnits;
        private Transform[] _archerPos;

        private event Action onAttackEvent;

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

        public override void TowerInit(MeshFilter consMeshFilter)
        {
            base.TowerInit(consMeshFilter);
            UnitDisable();
        }

        public override void TowerSetting(MeshFilter towerMeshFilter, int minDamage, int maxDamage, float range,
            float delay)
        {
            base.TowerSetting(towerMeshFilter, minDamage, maxDamage, range, delay);

            BatchArcher();

            onAttackEvent = null;
            if (!IsUniqueTower || TowerUniqueLevel == 1)
            {
                onAttackEvent += () => ProjectileAttack().Forget();
            }
            else
            {
                onAttackEvent += BulletAttack;
            }
        }

        private void BatchArcher()
        {
            var count = TowerUniqueLevel == 1 ? 2 : 1;
            archerCount = count;
            for (var i = 0; i < count; i++)
            {
                var index = !IsUniqueTower ? TowerLevel : TowerUniqueLevel + 3 + i;
                _archerUnits[i] = StackObjectPool.Get<ArcherUnit>("ArcherUnit", _archerPos[index].position);
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
            onAttackEvent?.Invoke();
        }

        private async UniTaskVoid ProjectileAttack()
        {
            for (int i = 0; i < archerCount; i++)
            {
                _archerUnits[i].TargetUpdate(target, isTargeting);
                StackObjectPool.Get("ArrowShootSFX", transform);
                StackObjectPool.Get<Projectile>("ArcherProjectile", _archerUnits[i].transform.position)
                    .Init(target, Damage);
                await UniTask.Delay(500);
            }
        }

        private void BulletAttack()
        {
            _archerUnits[0].TargetUpdate(target, isTargeting);
            StackObjectPool.Get("BulletShootSFX", transform);
            StackObjectPool.Get<Bullet>("ArcherBullet", _archerUnits[0].transform.position).Init(target, Damage);
        }
    }
}