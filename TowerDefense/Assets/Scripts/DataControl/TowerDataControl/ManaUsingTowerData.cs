using BackendControl;
using UnityEngine;

namespace DataControl.TowerDataControl
{
    [CreateAssetMenu(menuName = "Tower Data/Attack Tower Data/Mana Tower Data")]
    public class ManaUsingTowerData : AttackTowerData
    {
        [field: SerializeField] public byte initMana { get; private set; }
    }
}