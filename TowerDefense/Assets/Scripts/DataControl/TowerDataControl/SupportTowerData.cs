using UnityEngine;

namespace DataControl.TowerDataControl
{
    [CreateAssetMenu(menuName = "Tower Data/Support Tower Data")]
    public class SupportTowerData : TowerData
    {
        [field: SerializeField] public float towerUpdateCooldown { get; private set; }
    }
}
