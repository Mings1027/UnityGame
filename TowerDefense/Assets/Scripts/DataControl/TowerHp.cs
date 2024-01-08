using StatusControl;
using UnityEngine;

namespace DataControl
{
    [CreateAssetMenu(menuName = "PlayerData/TowerHp")]
    public class TowerHp : ScriptableObject
    {
        public int Hp { get; set; }
        public TowerHealth towerHealth { get; set; }
    }
}