using CustomEnumControl;
using ManagerControl;
using PoolObjectControl;
using TextControl;
using UIControl;

namespace ItemControl
{
    public class TowerHeartItem : ItemButton
    {
        public override void Spawn()
        {
            if (GameHUD.towerHealth.Current >= 10) return;
            PoolObjectManager.Get<FloatingText>(UIPoolObjectKey.TowerHealText, cameraManager.camPos).SetHpText(5);
            GameHUD.TowerHeal();
        }
    }
}