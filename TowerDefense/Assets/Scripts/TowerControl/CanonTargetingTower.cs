using DataControl;
using DG.Tweening;
using GameControl;
using PoolObjectControl;
using ProjectileControl;
using UnityEngine;

namespace TowerControl
{
    public class CanonTargetingTower : TargetingTower
    {
        private Sequence _atkSequence;
        private Transform _childObj;
        [SerializeField] private ParticleSystem canonSmoke;

        private void OnDestroy()
        {
            _atkSequence.Kill();
        }

        protected override void Init()
        {
            base.Init();
            _childObj = transform.GetChild(0);
            _atkSequence = DOTween.Sequence().SetAutoKill(false).Pause()
                .Append(_childObj.DOScaleY(0.5f, 0.3f).SetEase(Ease.OutQuint))
                .Append(_childObj.DOScaleY(1f, 0.3f).SetEase(Ease.OutQuint));
        }

        protected override void Attack()
        {
            _atkSequence.Restart();
            canonSmoke.Play();
            ProjectileInit(PoolObjectKey.CanonProjectile, transform.position + new Vector3(0, 2, 0));
        }

        protected override void ProjectileInit(PoolObjectKey poolObjKey, Vector3 firePos)
        {
            projectile = PoolObjectManager.Get<CanonProjectile>(poolObjKey, firePos);
            base.ProjectileInit(poolObjKey, firePos);
        }
    }
}