using CustomEnumControl;
using DataControl;
using UnityEngine;

namespace UnitControl.EnemyControl
{
    public class EnemyUnit : Unit
    {
        private Vector3 _prevPos;

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("BaseTower"))
            {
                gameObject.SetActive(false);
            }
        }

        protected override void Patrol()
        {
            base.Patrol();
            navMeshAgent.SetDestination(Vector3.zero);
        }

        public void SetAnimationSpeed(float animSpeed)
        {
            anim.speed = animSpeed;
        }

        public void SpawnInit(EnemyData enemyData)
        {
            unitState = UnitState.Patrol;
            navMeshAgent.speed = enemyData.Speed;
            SetAnimationSpeed(navMeshAgent.speed);
            atkDelay = enemyData.AttackDelay;
            damage = enemyData.Damage;
            navMeshAgent.SetDestination(Vector3.zero);
            anim.SetBool(IsWalk, true);
        }

        public void Stuck()
        {
            if (unitState == UnitState.Attack || Vector3.Distance(_prevPos, transform.position) >= 5) return;
            navMeshAgent.enabled = false;
            navMeshAgent.enabled = true;
            _prevPos = transform.position;
        }
    }
}