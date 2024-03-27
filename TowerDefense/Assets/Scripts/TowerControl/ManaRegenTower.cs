using UIControl;
using Utilities;

namespace TowerControl
{
    public class ManaRegenTower : SupportTower
    {
        private byte _manaRegenValue;

        public override void Init()
        {
            base.Init();
            updateCooldown = new Cooldown();
            _manaRegenValue = 1;
        }

        public override void LevelUp()
        {
            base.LevelUp();
            _manaRegenValue++;
        }

        public override void TowerUpdate()
        {
            if (updateCooldown.IsCoolingDown) return;
            GameHUD.towerMana.Heal(_manaRegenValue);
            updateCooldown.StartCooldown();
        }
    }
}