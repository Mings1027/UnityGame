using UnityEngine;

namespace UnitControl.EnemyControl
{
    public abstract class RangeEnemy : EnemyUnit
    {
        protected override void Update()
        {
            if (IsTargeting)
            {
                if (!attackAble) return;
                
                nav.isStopped = true;
                Attack();
                StartCoolDown().Forget();
            }
            else
            {
                if (nav.isStopped) nav.isStopped = false;
                nav.SetDestination(destination.position);
                if (Vector3.Distance(transform.position, destination.position) <= nav.stoppingDistance)
                    gameObject.SetActive(false);
            }
        }

        protected abstract void SpawnProjectile(Transform t);

        protected override void Attack()
        {
            SpawnProjectile(Target);
        }
    }
}