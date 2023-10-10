using DG.Tweening;
using UnityEngine;

namespace TowerControl
{
    public class BallistaTargetingTower : TargetingTower
    {
        private Vector3 _targetDirection;
        private Transform _ballista;
        private bool isAttacking;
        [SerializeField] private float smoothTurnSpeed;

        protected override void Init()
        {
            base.Init();
            _ballista = transform.Find("Ballista");
            firePos = _ballista.GetChild(0);
        }

        public override void TowerUpdate()
        {
            base.TowerUpdate();

            if (!isTargeting || isAttacking) return;

            var t = target.transform;
            var targetPos = t.position + t.forward;
            var dir = targetPos - transform.position;
            var rotGoal = Quaternion.LookRotation(dir) * Quaternion.Euler(-30f, 0f, 0f);
            _ballista.rotation = Quaternion.Slerp(_ballista.rotation, rotGoal, smoothTurnSpeed);
        }

        protected override void Attack()
        {
            isAttacking = true;
            _ballista.DOMove(_ballista.position - _ballista.forward * 0.2f, 0.1f).SetEase(Ease.OutExpo)
                .SetLoops(2, LoopType.Yoyo).OnComplete(() => isAttacking = false);
            base.Attack();
        }
    }
}