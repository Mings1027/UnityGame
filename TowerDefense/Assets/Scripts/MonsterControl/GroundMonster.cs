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

            base.MonsterUpdate();
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
    }
}