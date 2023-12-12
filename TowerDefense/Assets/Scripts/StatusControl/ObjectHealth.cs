using TMPro;
using UnityEngine;

namespace StatusControl
{
    public class ObjectHealth : Health
    {
        [SerializeField] private TMP_Text healthText;

        public override void Init(float amount)
        {
            base.Init(amount);
            healthText.text = Current + " / " + Initial;
        }

        public override void Damage(float amount)
        {
            base.Damage(amount);
            healthText.text = Current + " / " + Initial;
        }

        public override void Heal(in float amount)
        {
            base.Heal(in amount);
            healthText.text = Current + " / " + Initial;
        }
    }
}