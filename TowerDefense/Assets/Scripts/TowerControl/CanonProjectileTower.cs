using DG.Tweening;
using UnityEngine;

namespace TowerControl
{
    public class CanonProjectileTower : ProjectileTower
    {
        private Transform _childObj;
        [SerializeField] private ParticleSystem canonSmoke;

        protected override void Init()
        {
            base.Init();
            firePos = canonSmoke.transform;
            
            _childObj = transform.GetChild(0);
            atkSequence = DOTween.Sequence().SetAutoKill(false).Pause()
                .Append(_childObj.DOScaleY(0.5f, 0.3f).SetEase(Ease.OutQuint))
                .Append(_childObj.DOScaleY(1f, 0.3f).SetEase(Ease.OutQuint));
        }

        protected override void Attack()
        {
            base.Attack();
            canonSmoke.Play();
        }
    }
}