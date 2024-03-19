using CustomEnumControl;
using ManagerControl;
using PoolObjectControl;
using TextControl;
using UIControl;
using UnityEngine;

namespace ItemControl
{
    public class MoneyBagItem : ItemButton
    {
        public override bool Spawn()
        {
            PoolObjectManager.Get<FloatingText>(UIPoolObjectKey.MoneyText, cameraManager.camPos).SetGoldText(500);
            SoundManager.PlayUISound(SoundEnum.HighCost);
            GameHUD.IncreaseTowerGold(500);
            return true;
        }
    }
}