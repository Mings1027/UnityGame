using BackendControl;
using CustomEnumControl;
using UnityEngine;

namespace DataControl.TowerDataControl
{
    [CreateAssetMenu(menuName = "Tower Data/Attack Tower Data/Targeting Tower Data")]
    public class TargetingTowerData : AttackTowerData
    {
        [field: SerializeField] public PoolObjectKey poolObjectKey { get; private set; }
        [field: SerializeField] public ParticleSystem.MinMaxGradient[] projectileColor { get; private set; }
    }
}