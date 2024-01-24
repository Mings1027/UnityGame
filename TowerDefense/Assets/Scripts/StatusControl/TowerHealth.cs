using CustomEnumControl;
using PoolObjectControl;
using TextControl;
using TMPro;
using UnityEngine;

namespace StatusControl
{
    [RequireComponent(typeof(RectTransform))]
    public class TowerHealth : Health
    {
        [SerializeField] private TMP_Text healthText;

        public override void Init(float amount)
        {
            base.Init(amount);
            healthText.text = Current + " / " + Initial;
        }

        public override void Damage(int amount)
        {
            base.Damage(amount);
            healthText.text = Current + " / " + Initial;
        }

        public override void Heal(in ushort amount)
        {
            if (IsDead) return;
            if (Current >= Initial) return;
            Current += amount;

            healthText.text = Current + " / " + Initial;
        }
    }
}