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
        [SerializeField] private LayerMask towerLayer;
        [SerializeField] private DeBuffData deBuffData;

        protected override void Init()
        {
            base.Init();

            _atkSequence = DOTween.Sequence().SetAutoKill(false).Pause()
                .Append(crystal.DOLocalMoveY(crystal.position.y + 0.1f, 0.5f).SetEase(Ease.InOutSine))
                .SetLoops(2, LoopType.Yoyo);

            _crystalMeshFilter = crystal.GetComponent<MeshFilter>();

            effectName = new[] { "MageVfx1", "MageVfx2", "MageVfx3" };
        }

        public override void TowerSetting(MeshFilter towerMesh, int damageData, int rangeData,
            float attackDelayData)
        {
            base.TowerSetting(towerMesh, damageData, rangeData, attackDelayData);
            CrystalInit();
        }

        private void CrystalInit()
        {
            _crystalMeshFilter.sharedMesh = crystalMesh[TowerLevel];
            Physics.Raycast(transform.position + Vector3.up * 2, Vector3.down, out var hit, 2, towerLayer);
            crystal.position = hit.point + new Vector3(0, 0.5f, 0);
        }

        protected override void Attack()
        {
            _atkSequence.Restart();
            var p = ObjectPoolManager.Get<MageBullet>(StringManager.MageBullet, crystal.position);

            p.Init(target, damage, deBuffData.speedDeBuffData[TowerLevel]);

            EffectAttack(p.transform);
        }
    }
}