using ManagerControl;
using UIControl;

namespace TowerControl
{
    public class ManaRegenTower : SupportTower
    {
        public override void TowerSetting()
        {
            base.TowerSetting();
            GameHUD.BuildManaTower();
        }

        public override void DisableObject()
        {
            GameHUD.RemoveManaTower();
            base.DisableObject();
        }
    }
}