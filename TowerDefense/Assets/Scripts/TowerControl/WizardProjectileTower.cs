using DG.Tweening;
using ManagerControl;
using PoolObjectControl;
using ProjectileControl;
using UnityEngine;

namespace TowerControl
{
    public class WizardProjectileTower : ProjectileTower
    {
        private MeshFilter _crystalMeshFilter;
        [SerializeField] private Transform crystal;
        [SerializeField] private Mesh[] crystalMesh;

        protected override void Init()
        {
            base.Init();
            firePos = crystal;

            atkSequence = DOTween.Sequence().SetAutoKill(false).Pause()
                .Append(crystal.DOScale(1.2f, 0.5f).From(1).SetEase(Ease.InOutSine))
                .Join(crystal.DORotate(new Vector3(0, 360, 0), 0.5f, RotateMode.FastBeyond360))
                .Append(crystal.DOScale(1, 0.5f).From(1.2f).SetEase(Ease.InOutSine));

            _crystalMeshFilter = crystal.GetComponent<MeshFilter>();
            crystal.localScale = Vector3.zero;
        }

        protected override void Attack()
        {
            atkSequence.Restart();
            SoundManager.Play3DSound(audioClip, transform.position);
            var projectile = PoolObjectManager.Get<WizardProjectile>(projectileKey, firePos.position);
            projectile.Init(towerDamage, effectIndex, target);
            projectile.DeBuffInit(effectIndex);
        }

        public override void TowerSetting(MeshFilter towerMesh, int damageData, byte rangeData,
            float cooldownData)
        {
            base.TowerSetting(towerMesh, damageData, rangeData, cooldownData);
            CrystalInit();
        }

        private void CrystalInit()
        {
            _crystalMeshFilter.sharedMesh = crystalMesh[towerLevel];
            crystal.position = transform.position + new Vector3(0, boxCollider.size.y + 0.5f, 0);
            crystal.DOScale(1, 0.5f).From(0).SetEase(Ease.OutBack);
        }
    }
}