using CustomEnumControl;
using DataControl;
using UnityEngine;

namespace UnitControl.EnemyControl
{
    public class EnemyUnit : Unit
    {
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("BaseTower"))
            {
                gameObject.SetActive(false);
            }
        }

        protected override void Patrol()
        {
            navMeshAgent.SetDestination(Vector3.zero);
            anim.SetBool(IsWalk, true);
            base.Patrol();
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
    }
}