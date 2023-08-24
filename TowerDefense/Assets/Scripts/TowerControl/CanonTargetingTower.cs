using DataControl;
using DG.Tweening;
using GameControl;
using ProjectileControl;
using UnityEngine;

namespace TowerControl
{
    public class CanonTargetingTower : TargetingTower
    {
        private Sequence _atkSequence;

        [SerializeField] private ParticleSystem canonSmoke;
        
        private void OnDestroy()
        {
            _atkSequence.Kill();
        }

        protected override void Init()
        {
            base.Init();

            _atkSequence = DOTween.Sequence().SetAutoKill(false).Pause()
                .Append(meshFilter.transform.DOScaleY(0.5f, 0.3f).SetEase(Ease.OutQuint))
                .Append(meshFilter.transform.DOScaleY(1f, 0.3f).SetEase(Ease.OutQuint));
        }

        protected override void Attack()
        {
            _atkSequence.Restart();
            canonSmoke.Play();
            ObjectPoolManager.Get(PoolObjectName.CanonShootSfx, transform);
            ObjectPoolManager.Get<CanonProjectile>(PoolObjectName.CanonBullet,
                transform.position + new Vector3(0, 2, 0)).Init(target.position, damage);
        }
    }
}