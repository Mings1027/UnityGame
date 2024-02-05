using DG.Tweening;
using UnityEngine;

namespace TowerControl
{
    public class BallistaProjectileTower : ProjectileTower
    {
        private Vector3 _targetDirection;

        [SerializeField] private Transform ballista;
        [SerializeField] private float smoothTurnSpeed;

        protected override void Init()
        {
            base.Init();
            firePos = ballista;
            ballista.localScale = Vector3.zero;
            var ballistaChild = ballista.GetChild(0);
            atkSequence = DOTween.Sequence().SetAutoKill(false).Pause()
                .Append(ballistaChild.DOLocalMove(ballistaChild.forward * -0.2f, 0.2f)
                    .SetEase(Ease.OutExpo).SetLoops(2, LoopType.Yoyo));
        }

        public override void TowerUpdate()
        {
            base.TowerUpdate();
            if (!isTargeting || !target) return;
            var t = target.transform;
            var targetPos = t.position + t.forward;
            var dir = targetPos - transform.position;
            var rotGoal = Quaternion.LookRotation(dir) * Quaternion.Euler(-45f, 0f, 0f);
            ballista.rotation = Quaternion.Slerp(ballista.rotation, rotGoal, smoothTurnSpeed);
        }

        public override void TowerSetting(MeshFilter towerMesh, int damageData, byte rangeData, float cooldownData)
        {
            base.TowerSetting(towerMesh, damageData, rangeData, cooldownData);
            BallistaInit();
        }

        private void BallistaInit()
        {
            ballista.position = transform.position + new Vector3(0, boxCollider.size.y, 0);
            ballista.DOScale(1, 0.5f).From(0).SetEase(Ease.OutBack);
        }
    }
}