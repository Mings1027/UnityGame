using UIManager = ManagerControl.UIManager;

namespace TowerControl
{
    public class SupportTower : Tower
    {
        public override void DisableObject()
        {
            UIManager.instance.RemoveManaTower();
            base.DisableObject();
        }
    }
}