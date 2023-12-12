using GameControl;
using UnityEngine;

namespace DataControl
{
    [CreateAssetMenu(menuName = "Tower Data/Battle Tower Data/Mana Tower Data")]
    public class ManaTowerData : BattleTowerData
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

            // var mana = PlayerPrefs.GetInt(StringManager.Mana);
            // if (mana <= 0)
            // {
            //     PlayerPrefs.SetInt(StringManager.Mana, initMana);
            //     attackMana = initMana;
            // }
            // else
            // {
            //     attackMana = mana;
            // }
        }
    }
}