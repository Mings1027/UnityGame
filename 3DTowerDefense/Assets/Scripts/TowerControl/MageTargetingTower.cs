using System;
using DataControl;
using DG.Tweening;
using GameControl;
using UnityEngine;
using WeaponControl;

namespace TowerControl
{
    public class MageTargetingTower : TargetingTower
    {
        private Sequence _atkSequence;
        private Material _material;
        private MeshFilter _crystalMeshFilter;

        private event Action onAttackEvent;

        [SerializeField] private Transform crystal;
        [SerializeField] private Mesh[] crystalMesh;
        [SerializeField] private Transform[] crystalPositions;
        private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");

        protected override void Init()
        {
            base.Init();
            targetColliders = new Collider[3];
            _material = crystal.GetComponent<Renderer>().material;
            _crystalMeshFilter = crystal.GetComponent<MeshFilter>();

            _atkSequence = DOTween.Sequence().SetAutoKill(false).Pause()
                .Append(_material.DOColor(_material.GetColor(EmissionColor) * 2, 0.5f))
                .Append(_material.DOColor(_material.GetColor(EmissionColor), 0.5f));
        }

        public override void TowerInit(MeshFilter consMeshFilter)
        {
            base.TowerInit(consMeshFilter);
            // 타워 업그레이드 중에 crystal 보이면 어색하기 때문에 crystal을 타워밑으로 옮겨 잠시 숨겨줌
            crystal.position = transform.position;
        }

        public override void TowerSetting(MeshFilter towerMeshFilter, int minDamage, int maxDamage, float range,
            float delay, float health = 0)
        {
            base.TowerSetting(towerMeshFilter, minDamage, maxDamage, range, delay, health);
            CrystalPosInit();
            onAttackEvent = null;
            onAttackEvent += IsUniqueTower ? TowerUniqueLevel == 0 ? SlowDownAttack : PenetrationAttack : NormalAttack;
            // if (IsUniqueTower)
            // {
            //     if (TowerUniqueLevel == 0)
            //     {
            //         onAttackEvent += SlowDownAttack;
            //     }
            //     else
            //     {
            //         onAttackEvent += PenetrationAttack;
            //     }
            // }
            // else
            // {
            //     onAttackEvent += NormalAttack;
            // }
        }

        private void CrystalPosInit()
        {
            var index = IsUniqueTower ? TowerUniqueLevel + 3 : TowerLevel;
            crystal.position = crystalPositions[index].position;
            _crystalMeshFilter.sharedMesh = crystalMesh[index];
        }

        protected override void Attack()
        {
            _atkSequence.Restart();
            onAttackEvent?.Invoke();
        }

        private void NormalAttack()
        {
            ObjectPoolManager.Get(PoolObjectName.MageShootSfx, transform);
            ObjectPoolManager.Get<MageBullet>(PoolObjectName.MageBullet, crystalPositions[TowerLevel])
                .Init(target, Damage);
        }

        private void SlowDownAttack()
        {
            ObjectPoolManager.Get(PoolObjectName.SlowDownMageShootSfx, transform);
            ObjectPoolManager
                .Get<SlowDownMageBullet>(PoolObjectName.SlowDownMageBullet, crystalPositions[TowerUniqueLevel + 3])
                .Init(target, Damage);
        }

        private void PenetrationAttack()
        {
            ObjectPoolManager.Get(PoolObjectName.PenetrationMageShootSfx, transform);
            var p = ObjectPoolManager.Get<PenetrationMageBullet>(PoolObjectName.PenetrationMageBullet,
                crystalPositions[TowerUniqueLevel + 3]);
            p.Init(target, Damage);

            var size = Physics.OverlapSphereNonAlloc(transform.position, TowerRange, targetColliders, TargetLayer);

            p.UpdateTarget(size, targetColliders);
            //범위에 들어온 적 순서데로 발사체가 관통하며 공격 전기 이펙트 처럼 하면 좋겟는데 어케 만들지..
        }
    }
}