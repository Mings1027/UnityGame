using CustomEnumControl;
using PoolObjectControl;

namespace StatusControl
{
    public class UnitHealth : Health
    {
        public override void Damage(int amount)
        {
            base.Damage(amount);

            PoolObjectManager.Get(PoolObjectKey.BloodVfx, transform.position);
        }
    }
}