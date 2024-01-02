using UnityEngine;
using Utilities;

namespace DataControl.TowerDataControl
{
    [CreateAssetMenu(menuName = "Tower Data/Attack Tower Data/Mana Tower Data")]
    public class ManaUsingTowerData : AttackTowerData
    {
        public float attackMana { get; private set; }
        [SerializeField] private int initMana;

        public override void InitState()
        {
            base.InitState();

            var attackRange = PlayerPrefs.GetInt(StringManager.RangeDic[TowerType]);
            if (attackRange <= 0)
            {
                PlayerPrefs.SetInt(StringManager.RangeDic[TowerType], InitRange);
                AttackRange = InitRange;
            }
            else
            {
                AttackRange = (byte)attackRange;
            }

            attackMana = initMana;
        }
    }
}