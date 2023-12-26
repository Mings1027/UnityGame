using StatusControl;
using UnityEngine;

[CreateAssetMenu(menuName = "PlayerData/TowerMana")]
public class TowerMana : ScriptableObject
{
    public int Mana { get; set; }
    public Mana towerMana { get; set; }
}