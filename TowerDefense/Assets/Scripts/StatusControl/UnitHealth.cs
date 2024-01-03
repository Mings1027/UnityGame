using CustomEnumControl;
using PoolObjectControl;

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