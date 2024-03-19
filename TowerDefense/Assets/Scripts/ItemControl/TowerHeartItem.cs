using CustomEnumControl;
using PoolObjectControl;
using TextControl;
using UIControl;

namespace ItemControl
{
    public class TowerHeartItem : ItemButton
    {
        public override bool Spawn()
        {
            if (GameHUD.towerHealth.Current < 10)
            {
                PoolObjectManager.Get<FloatingText>(UIPoolObjectKey.TowerHealText, cameraManager.camPos).SetHpText(5);
                GameHUD.TowerHeal();
                return true;
            }

            return false;
        }
    }
}