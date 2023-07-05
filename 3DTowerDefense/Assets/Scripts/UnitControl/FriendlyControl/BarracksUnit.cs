using DataControl;
using GameControl;
using UnityEngine;
using WeaponControl;

namespace UnitControl.FriendlyControl
{
    public class BarracksUnit : FriendlyUnit
    {
        protected override void Attack()
        {
            if (target.TryGetComponent(out Health h))
            {
                h.TakeDamage(Damage);
            }
        }
    }
}