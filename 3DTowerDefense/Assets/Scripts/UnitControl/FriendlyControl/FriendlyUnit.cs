using UnityEngine;

namespace UnitControl.FriendlyControl
{
    public abstract class FriendlyUnit : Unit
    {
        protected override void UnityUpdate()
        {
            base.UnityUpdate();
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