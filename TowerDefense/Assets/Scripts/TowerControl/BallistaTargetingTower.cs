using Cysharp.Threading.Tasks;
using DataControl;
using GameControl;
using ProjectileControl;
using UnityEngine;

namespace TowerControl
{
    public class BallistaTargetingTower : TargetingTower
    {
        private int _archerCount;
        private Vector3 _targetDirection;
        private Transform _secondClosestTarget;

        [SerializeField] private Transform ballista;
        [SerializeField] private Transform firePos;
        [SerializeField] private float smoothTurnSpeed;

        protected override void OnEnable()
        {
            base.OnEnable();
            onAttackEvent += OneShotAttack;
        }

        private void LateUpdate()
        {
            if (!isTargeting) return;

            var targetPos = target.position + target.forward;
            var dir = targetPos - transform.position;
            var rotGoal = Quaternion.LookRotation(dir) * Quaternion.Euler(-30f, 0f, 0f);
            ballista.rotation = Quaternion.Slerp(ballista.rotation, rotGoal, smoothTurnSpeed);
        }

        protected override void Init()
        {
            base.Init();
            targetColliders = new Collider[5];
        }

        private void OneShotAttack()
        {
            ObjectPoolManager.Get(PoolObjectName.ArrowShootSfx, transform);
            var bullet = ObjectPoolManager.Get<BallistaProjectile>(PoolObjectName.BallistaProjectile, firePos.position);
            bullet.Init(target, damage);
        }

    }
}