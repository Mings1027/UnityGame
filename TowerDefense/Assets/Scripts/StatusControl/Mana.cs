using InterfaceControl;
using TMPro;
using UnityEngine;

namespace StatusControl
{
    public class Mana : Progressive, IDamageable, IHealable
    {
        [SerializeField] private TMP_Text manaText;

        public override void Init(float amount)
        {
            base.Init(amount);
            manaText.text = Current + " / " + Initial;
        }

        public void Damage(int amount)
        {
            Current -= amount;
            manaText.text = Current + " / " + Initial;
        }

        public void Heal(in ushort amount)
        {
            if (IsFull) return;
            Current += amount;
            if (Current > Initial) Current = Initial;
            manaText.text = Current + " / " + Initial;
        }
    }
}