using CustomEnumControl;
using PoolObjectControl;
using UnityEngine;

namespace StatusControl
{
    public class UnitHealth : Health
    {
        private Collider _col;

        private void Awake()
        {
            _col = GetComponent<Collider>();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            _col.enabled = true;
        }

        public override void Damage(in float amount)
        {
            base.Damage(in amount);

            PoolObjectManager.Get(PoolObjectKey.BloodVfx, transform.position);
            if (!IsDead) return;
            _col.enabled = false;
        }
    }
}