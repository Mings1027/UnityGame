using CustomEnumControl;
using UnityEngine;

namespace MonsterControl
{
    public class GroundMonster : MonsterUnit
    {
        protected override void Awake()
        {
            base.Awake();
            targetLayer = LayerMask.GetMask("Unit");
            targetCollider = new Collider[attackTargetCount];
        }

        public override void Init()
        {
            base.Init();
            patrolCooldown.cooldownTime = 0.5f;
        }

        public override void MonsterUpdate()
        {
            if (!navMeshAgent.enabled) return;
            if (health.IsDead) return;
            switch (unitState)
            {
                case UnitState.Patrol:
                    Patrol();
                    break;
                case UnitState.Chase:
                    Chase();
                    break;
                case UnitState.Attack:
                    Attack();
                    break;
            }

            if (target && target.enabled)
            {
                var dir = (target.transform.position - transform.position).normalized;
                var targetRot = Quaternion.LookRotation(dir);
                var eulerAngleDiff = targetRot.eulerAngles - transform.rotation.eulerAngles;
                transform.Rotate(eulerAngleDiff);
            }

            base.MonsterUpdate();
        }

        protected override void Patrol()
        {
            if (patrolCooldown.IsCoolingDown) return;
            var size = Physics.OverlapSphereNonAlloc(transform.position, sightRange, targetCollider, targetLayer);
            if (size <= 0)
            {
                target = null;
                if (navMeshAgent.isOnNavMesh)
                {
                    navMeshAgent.SetDestination(Vector3.zero);
                }

                return;
            }

            var shortestDistance = float.MaxValue;
            for (var i = 0; i < size; i++)
            {
                var distanceToResult = Vector3.SqrMagnitude(transform.position - targetCollider[i].bounds.center);
                if (shortestDistance > distanceToResult)
                {
                    shortestDistance = distanceToResult;
                    target = targetCollider[i];
                }
            }

            unitState = UnitState.Chase;
            patrolCooldown.StartCooldown();
        }

        private void Chase()
        {
            if (!target.enabled)
            {
                unitState = UnitState.Patrol;
                return;
            }

            if (navMeshAgent.isOnNavMesh)
                navMeshAgent.SetDestination(target.transform.position + Random.insideUnitSphere * atkRange);
            if (Vector3.Distance(target.transform.position, transform.position) <= atkRange)
            {
                unitState = UnitState.Attack;
            }
        }

        protected override void Attack()
        {
            if (!target || !target.enabled ||
                Vector3.Distance(target.transform.position, transform.position) > atkRange)
            {
                unitState = UnitState.Patrol;
                return;
            }

            if (attackCooldown.IsCoolingDown) return;

            anim.SetTrigger(isAttack);
            TryDamage();

            attackCooldown.StartCooldown();
        }
    }
}