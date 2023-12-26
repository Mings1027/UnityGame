using StatusControl;
using UnityEngine;

[CreateAssetMenu(menuName = "PlayerData/TowerHp")]
public class TowerHp : ScriptableObject
{
    public int Hp { get; set; }
    public TowerHealth towerHealth { get; set; }
}