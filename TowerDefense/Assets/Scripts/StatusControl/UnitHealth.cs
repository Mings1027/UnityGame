using CustomEnumControl;
using PoolObjectControl;
using UnityEngine;

namespace StatusControl
{
    public class UnitHealth : Health
    {
        public override void Damage(float amount)
        {
            base.Damage(amount);

            PoolObjectManager.Get(PoolObjectKey.BloodVfx, transform.position);
        }
    }
}