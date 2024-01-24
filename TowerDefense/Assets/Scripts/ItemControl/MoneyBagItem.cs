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
        protected override void Awake()
        {
            base.Awake();
            itemType = ItemType.MoneyBag;
        }

        public override void Spawn()
        {
            PoolObjectManager.Get<FloatingText>(UIPoolObjectKey.MoneyText, CameraManager.camPos).SetGoldText(500);
            SoundManager.PlayUISound(SoundEnum.HighCost);
            UIManager.instance.towerGold += 500;
        }
    }
}