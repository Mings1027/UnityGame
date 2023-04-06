using UnityEngine;

namespace UnitControl.FriendlyControl
{
    public abstract class FriendlyUnit : Unit
    {
        private void Update()
        {
            if (gameManager.IsPause) return;
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