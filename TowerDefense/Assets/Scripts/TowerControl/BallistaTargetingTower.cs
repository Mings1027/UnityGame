using DG.Tweening;
using UnityEngine;

namespace TowerControl
{
    public class BallistaTargetingTower : TargetingTower
    {
        private Vector3 _targetDirection;

        [SerializeField] private Transform ballista;
        [SerializeField] private float smoothTurnSpeed;

        private void LateUpdate()
        {
            if (!IsTargeting) return;

            var t = Target.transform;
            var targetPos = t.position + t.forward;
            var dir = targetPos - transform.position;
            var rotGoal = Quaternion.LookRotation(dir) * Quaternion.Euler(-45f, 0f, 0f);
            ballista.rotation = Quaternion.Slerp(ballista.rotation, rotGoal, smoothTurnSpeed);
        }

        protected override void Init()
        {
            base.Init();
            FirePos = ballista;
            ballista.localScale = Vector3.zero;
        }

        public override void TowerSetting(MeshFilter towerMesh, int damageData, byte rangeData, float attackDelayData)
        {
            base.TowerSetting(towerMesh, damageData, rangeData, attackDelayData);
            BallistaInit();
        }

        private void BallistaInit()
        {
            ballista.position = transform.position + new Vector3(0, BoxCollider.size.y, 0);
            ballista.DOScale(1, 0.5f).From(0).SetEase(Ease.OutBack);
        }

        protected override void Attack()
        {
            ballista.DOMove(ballista.position - ballista.forward * 0.2f, 0.2f).SetEase(Ease.OutExpo)
                .SetLoops(2, LoopType.Yoyo);
            base.Attack();
        }
    }
}