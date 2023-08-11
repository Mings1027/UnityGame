using System;
using DataControl;
using DG.Tweening;
using GameControl;
using ProjectileControl;
using UnityEngine;

namespace TowerControl
{
    public class MageTargetingTower : TargetingTower
    {
        private Sequence _atkSequence;
        private MeshFilter _crystalMeshFilter;
        [SerializeField] private Transform crystal;
        [SerializeField] private Mesh[] crystalMesh;
        [SerializeField] private Transform[] crystalPositions;

        protected override void OnEnable()
        {
            base.OnEnable();
            onAttackEvent += NormalAttack;
        }

        protected override void Init()
        {
            base.Init();
            targetColliders = new Collider[3];
            _crystalMeshFilter = crystal.GetComponent<MeshFilter>();

            _atkSequence = DOTween.Sequence().SetAutoKill(false).Pause();
            //공격할때 크리스탈이 위로 떠오르면서 회전하는 시퀀스 넣기
        }

        public override void TowerSetting(MeshFilter towerMesh, int damageData, int attackRangeData,
            float attackDelayData)
        {
            base.TowerSetting(towerMesh, damageData, attackRangeData, attackDelayData);
            CrystalPosInit();
        }

        private void CrystalPosInit()
        {
            var index = TowerLevel;
            crystal.position = crystalPositions[index].position;
            _crystalMeshFilter.sharedMesh = crystalMesh[index];
        }

        protected override void Attack()
        {
            base.Attack();
            _atkSequence.Restart();
        }

        private void NormalAttack()
        {
            ObjectPoolManager.Get(PoolObjectName.MageShootSfx, transform);
            ObjectPoolManager.Get<MageBullet>(PoolObjectName.MageBullet, crystalPositions[TowerLevel])
                             .Init(target, damage);
        }
    }
}