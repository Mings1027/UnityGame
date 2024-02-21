using CustomEnumControl;
using ManagerControl;
using PoolObjectControl;
using TextControl;

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
            if (UIManager.GetTowerHealth().Current >= 10) return;
            PoolObjectManager.Get<FloatingText>(UIPoolObjectKey.TowerHealText, cameraManager.camPos).SetHpText(5);
            UIManager.TowerHeal();
        }
    }
}