using CustomEnumControl;
using UnityEngine;

namespace TowerControl
{
    public class ContinuousDamageTower : AttackTower
    {
        protected Collider[] targetColliders;
        protected TowerState towerState;
        protected Collider target;
        protected Transform firePos;

        [SerializeField] protected AudioClip audioClip;
        [SerializeField, Range(1, 5)] private byte targetColliderCount;

        protected override void Init()
        {
            base.Init();
            targetColliders = new Collider[targetColliderCount];
            patrolCooldown.cooldownTime = 0.1f;
        }

        public override void TowerUpdate()
        {
            switch (towerState)
            {
                case TowerState.Detect:
                    Detect();
                    break;
                case TowerState.Attack:
                    ReadyToAttack();
                    break;
            }
        }

        public override void TowerTargetInit()
        {
            towerState = TowerState.Detect;
            target = null;
        }

        protected virtual void Detect()
        {
        }

        protected virtual void ReadyToAttack()
        {
        }

        protected virtual void Attack()
        {
        }

        protected virtual void Hit()
        {
        }
    }
}