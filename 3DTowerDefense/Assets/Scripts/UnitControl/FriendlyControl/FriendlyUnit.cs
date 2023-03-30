using UnityEngine;

namespace UnitControl.FriendlyControl
{
    public abstract class FriendlyUnit : Unit
    {
        protected virtual void Update()
        {
            if (!isTargeting) return;
            if (attackAble && Vector3.Distance(transform.position, target.position) <= nav.stoppingDistance)
            {
                Attack();
                StartCoolDown();
            }
            else
            {
                nav.SetDestination(target.position);
            }
        }
    }
}