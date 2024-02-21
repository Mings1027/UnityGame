using ManagerControl;

namespace TowerControl
{
    public class ManaRegenTower : SupportTower
    {
        public override void TowerSetting()
        {
            base.TowerSetting();
            UIManager.BuildManaTower();
        }

        public override void DisableObject()
        {
            UIManager.RemoveManaTower();
            base.DisableObject();
        }
    }
}