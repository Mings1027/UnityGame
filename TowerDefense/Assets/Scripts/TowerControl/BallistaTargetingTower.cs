using DG.Tweening;
using UnityEngine;

namespace TowerControl
{
    public class BallistaTargetingTower : TargetingTower
    {
        private Vector3 _targetDirection;
        private Transform _ballista;

        [SerializeField] private float smoothTurnSpeed;

        protected override void Init()
        {
            base.Init();
            _ballista = transform.GetChild(0).Find("Ballista");
            firePos = _ballista.GetChild(0);
        }

        private void LateUpdate()
        {
            if (!isTargeting) return;

            var t = target.transform;
            var targetPos = t.position + t.forward;
            var dir = targetPos - transform.position;
            var rotGoal = Quaternion.LookRotation(dir) * Quaternion.Euler(-45f, 0f, 0f);
            _ballista.rotation = Quaternion.Slerp(_ballista.rotation, rotGoal, smoothTurnSpeed);
        }

        protected override void Attack()
        {
            _ballista.DOMove(_ballista.position - _ballista.forward * 0.2f, 0.2f).SetEase(Ease.OutExpo)
                .SetLoops(2, LoopType.Yoyo);
            base.Attack();
        }
    }
}