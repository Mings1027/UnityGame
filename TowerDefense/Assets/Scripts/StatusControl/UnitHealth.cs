using CustomEnumControl;
using PoolObjectControl;
using UnityEngine;

namespace StatusControl
{
    public class UnitHealth : Health
    {
        private Collider col;

        private void Awake()
        {
            col = GetComponent<Collider>();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            col.enabled = true;
        }

        public override void Damage(in float amount)
        {
            base.Damage(in amount);

            PoolObjectManager.Get(PoolObjectKey.BloodVfx, transform.position);
            if (!IsDead) return;
            col.enabled = false;
        }
    }
}