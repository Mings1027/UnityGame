using CustomEnumControl;
using ManagerControl;
using PoolObjectControl;
using TextControl;
using UIControl;
using UnityEngine;

namespace ItemControl
{
    public class TowerHeartItem : ItemButton
    {
        protected override void Awake()
        {
            base.Awake();
            itemType = ItemType.TowerHeart;
        }

        public override void Spawn()
        {
            PoolObjectManager.Get<FloatingText>(UIPoolObjectKey.TowerHealText, CameraManager.camPos).SetHpText(5);
            UIManager.instance.GetTowerHealth().Heal(5);
        }
    }
}