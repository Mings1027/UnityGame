using UnityEngine;

namespace UnitControl.FriendlyControl
{
    public abstract class FriendlyUnit : Unit
    {
        protected override void Update()
        {
            if (gameManager.IsPause) return;
            if (!IsTargeting) return;
            if (attackAble && Vector3.Distance(transform.position, Target.position) <= nav.stoppingDistance)
            {
                Attack();
                StartCoolDown();
            }
            else
            {
                nav.SetDestination(Target.position);
            }
        }
    }
}