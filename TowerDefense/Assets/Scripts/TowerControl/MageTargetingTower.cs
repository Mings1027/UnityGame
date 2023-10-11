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

        private void OnDestroy()
        {
            _atkSequence?.Kill();
        }

        protected override void Init()
        {
            base.Init();
            firePos = crystal;
            crystal.localScale = Vector3.zero;

            _atkSequence = DOTween.Sequence().SetAutoKill(false).Pause()
                .Append(crystal.DOScale(1.2f, 0.5f).SetEase(Ease.InOutSine))
                .Join(crystal.DORotate(new Vector3(0, 360, 0), 0.5f, RotateMode.FastBeyond360))
                .Append(crystal.DOScale(1,0.5f).SetEase(Ease.InOutSine));

            _crystalMeshFilter = crystal.GetComponent<MeshFilter>();
        }

        public override void TowerSetting(MeshFilter towerMesh, ushort damageData, byte rangeData,
            float attackDelayData)
        {
            base.TowerSetting(towerMesh, damageData, rangeData, attackDelayData);
            CrystalInit();
        }

        private void CrystalInit()
        {
            _crystalMeshFilter.sharedMesh = crystalMesh[TowerLevel];
            var newY = boxCollider.size.y + 0.3f;
            crystal.position = transform.position + new Vector3(0, newY, 0);
            crystal.DOScale(1, 0.5f).From(0).SetEase(Ease.OutBack);
        }

        protected override void Attack()
        {
            _atkSequence.Restart();
            base.Attack();
        }
    }
}