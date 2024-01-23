using CustomEnumControl;
using ManagerControl;
using PoolObjectControl;
using UIControl;
using UnityEngine;

namespace ItemControl
{
    public class MoneyBagItem : ItemButton
    {
        public override void Spawn()
        {
            var pos = Camera.main.ScreenToWorldPoint(Input.touches[0].position);
            PoolObjectManager.Get(PoolObjectKey.MoneyImage, pos);
            SoundManager.PlayUISound(SoundEnum.HighCost);
            UIManager.instance.towerGold += 500;
        }
    }
}