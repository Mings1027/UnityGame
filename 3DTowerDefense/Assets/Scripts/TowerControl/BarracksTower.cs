using System;
using DG.Tweening;
using GameControl;
using UnityEngine;

namespace TowerControl
{
    public class BarracksTower : Tower
    {
        private Tweener _attackTween;
        [SerializeField] private Ease atkEase;
        [SerializeField] private Cooldown _cooldown;
        [SerializeField] private Transform tipTarget;
        [SerializeField] private float atkDelay;

        public override void OnEnable()
        {
            base.OnEnable();
        }

        public override void OnDisable()
        {
            base.OnDisable();
        }
        // private void Awake()
        // {
        //     _attackTween = tipTarget.DOMove(tipTarget.position, atkDelay).SetAutoKill(false)
        //         .SetLoops(2,LoopType.Yoyo)
        //         .SetEase(atkEase);
        // }

        // private void OnEnable()
        // {
        //     InvokeRepeating(nameof(UpdateTarget), 0f, 0.5f);
        // }

        // private void FixedUpdate()
        // {
        //     if (_cooldown.IsCoolingDown) return;
        //     _attackTween.ChangeEndValue(target.position, atkDelay).Restart();
        //     _cooldown.StartCoolDown();
        // }

        // private void OnDisable()
        // {
        //     StackObjectPool.ReturnToPool(gameObject);
        // }
    }
}