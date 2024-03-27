using Utilities;

namespace TowerControl
{
    public abstract class SupportTower : Tower
    {
        protected Cooldown updateCooldown;

        public virtual void TowerSetting(float towerUpdateCooldown)
        {
            updateCooldown.cooldownTime = towerUpdateCooldown;
        }
    }
}