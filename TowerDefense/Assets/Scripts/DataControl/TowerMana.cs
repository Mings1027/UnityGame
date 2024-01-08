using StatusControl;
using UnityEngine;

namespace DataControl
{
    [CreateAssetMenu(menuName = "PlayerData/TowerMana")]
    public class TowerMana : ScriptableObject
    {
        public int Mana { get; set; }
        public Mana towerMana { get; set; }
    }
}