using CustomEnumControl;
using PoolObjectControl;
using UnityEngine;

namespace StatusControl
{
    public class UnitHealth : Health
    {
        public override void Damage(in float amount)
        {
            base.Damage(in amount);

            PoolObjectManager.Get(PoolObjectKey.BloodVfx, transform.position);
        }
    }
}