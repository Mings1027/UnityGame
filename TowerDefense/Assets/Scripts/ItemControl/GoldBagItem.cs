using CustomEnumControl;
using ManagerControl;
using PoolObjectControl;
using TextControl;
using UIControl;
using UnityEngine;

namespace ItemControl
{
    public class GoldBagItem : ItemButton
    {
        public override bool Spawn()
        {
            for (var i = 0; i < 5; i++)
            {
                PoolObjectManager
                    .Get<FloatingText>(UIPoolObjectKey.ItemGoldText, cameraManager.camPos + Random.insideUnitSphere * 10)
                    .SetGoldText(100);
            }

            SoundManager.PlayUISound(SoundEnum.HighCost);
            GameHUD.IncreaseTowerGold(500);
            return true;
        }
    }
}