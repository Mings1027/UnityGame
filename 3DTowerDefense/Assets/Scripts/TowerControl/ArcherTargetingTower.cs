using System;
using Cysharp.Threading.Tasks;
using DataControl;
using DG.Tweening;
using GameControl;
using UnityEngine;
using WeaponControl;

namespace TowerControl
{
    public class ArcherTargetingTower : TargetingTower
    {
        private int _archerCount;

        [SerializeField] private GameObject[] archerUnits;
        [SerializeField] private Vector3[] archerPos;
        [SerializeField, Range(0, 1)] private float smoothTurnSpeed;

        protected override void OnEnable()
        {
            base.OnEnable();
            onAttackEvent = null;
            onAttackEvent += OneArcherAttack;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            UnitDisable();
        }

        private void LateUpdate()
        {
            if (!isTargeting) return;
            ArcherLookTarget();
        }

        protected override void Init()
        {
            base.Init();
            targetColliders = new Collider[5];
        }

        public override void BuildTowerWithDelay(MeshFilter consMeshFilter, int minDamage, int maxDamage,
            float attackRange, float attackDelay, float health = 0)
        {
            base.BuildTowerWithDelay(consMeshFilter, minDamage, maxDamage, attackRange, attackDelay, health);

            _archerCount = TowerUniqueLevel == 1 ? 2 : 1;
            for (var i = 0; i < _archerCount; i++)
            {
                archerUnits[i].SetActive(false);
            }
        }

        public override void BuildTower(MeshFilter towerMeshFilter)
        {
            base.BuildTower(towerMeshFilter);

            BatchArcher();
            if (IsUniqueTower)
            {
                if (TowerUniqueLevel == 1)
                {
                    onAttackEvent = null;
                    onAttackEvent += () => TwoArcherAttack().Forget();
                }
                else
                {
                    onAttackEvent = null;
                    onAttackEvent += BulletAttack;
                }
            }

            print(onAttackEvent.Method);
        }

        private void BatchArcher()
        {
            for (var i = 0; i < _archerCount; i++)
            {
                var index = IsUniqueTower ? TowerUniqueLevel + 3 + i : TowerLevel;
                archerUnits[i].transform.localPosition = archerPos[index];
                archerUnits[i].SetActive(true);
            }
        }

        private void ArcherLookTarget()
        {
            var targetPos = target.position + target.forward;
            for (var i = 0; i < _archerCount; i++)
            {
                var dir = targetPos - transform.position;
                var yRot = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
                var lookRot = Quaternion.Euler(0, yRot, 0);
                archerUnits[i].transform.rotation =
                    Quaternion.Lerp(archerUnits[i].transform.rotation, lookRot, smoothTurnSpeed);
            }
        }

        private void UnitDisable()
        {
            for (var i = 0; i < archerUnits.Length; i++)
            {
                if (archerUnits[i].gameObject.activeSelf) archerUnits[i].SetActive(false);
            }
        }

        private void OneArcherAttack()
        {
            ObjectPoolManager.Get(PoolObjectName.ArrowShootSfx, transform);
            ObjectPoolManager.Get<ArcherProjectile>(PoolObjectName.ArcherProjectile, archerUnits[0].transform.position)
                .Init(target, Damage);
        }

        private async UniTaskVoid TwoArcherAttack()
        {
            OneArcherAttack();
            await UniTask.Delay(500);
            if (!target.gameObject.activeSelf) return;
            ObjectPoolManager.Get(PoolObjectName.ArrowShootSfx, transform);
            ObjectPoolManager.Get<ArcherProjectile>(PoolObjectName.ArcherProjectile, archerUnits[1].transform.position)
                .Init(target, Damage);
        }

        private void BulletAttack()
        {
            ObjectPoolManager.Get(PoolObjectName.BulletShootSfx, transform);
            ObjectPoolManager.Get<Bullet>(PoolObjectName.ArcherBullet, archerUnits[0].transform.position)
                .Init(target, Damage);
        }
    }
}