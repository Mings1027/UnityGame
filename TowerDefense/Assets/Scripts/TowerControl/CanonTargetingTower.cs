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

            effectName = new[] { "CanonVfx1", "CanonVfx2", "CanonVfx3" };
        }

        protected override void Attack()
        {
            _atkSequence.Restart();
            canonSmoke.Play();
            var bullet = ObjectPoolManager.Get<CanonProjectile>(StringManager.CanonProjectile,
                transform.position + new Vector3(0, 2, 0));
            var t = target.transform;
            bullet.Init(t.position + t.forward * 2, damage);

            EffectAttack(bullet.transform);
        }
    }
}