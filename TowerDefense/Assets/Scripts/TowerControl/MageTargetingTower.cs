using DG.Tweening;
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

        private void OnDestroy()
        {
            _atkSequence.Kill();
        }

        protected override void Init()
        {
            base.Init();
            firePos = crystal;

            _atkSequence = DOTween.Sequence().SetAutoKill(false).Pause()
                .Append(crystal.DOLocalMoveY(crystal.position.y + 0.1f, 0.5f).SetEase(Ease.InOutSine))
                .SetLoops(2, LoopType.Yoyo);

            _crystalMeshFilter = crystal.GetComponent<MeshFilter>();
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
            base.Attack();
        }
    }
}