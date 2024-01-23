using CustomEnumControl;
using ManagerControl;
using PoolObjectControl;
using UIControl;
using UnityEngine;

namespace ItemControl
{
    public class TowerHeartItem : ItemButton
    {
        public override void Spawn()
        {
            var pos = Camera.main.ScreenToWorldPoint(Input.touches[0].position);
            PoolObjectManager.Get(PoolObjectKey.HealImage, pos);
            UIManager.instance.GetTowerHealth().Heal(5);
        }
    }
}