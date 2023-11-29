using CustomEnumControl;
using UnityEngine;

namespace DataControl
{
    [CreateAssetMenu]
    public class TargetingTowerData : TowerData
    {
        public ParticleSystem.MinMaxGradient[] ProjectileColor => projectileColor;
        public PoolObjectKey PoolObjectKey => poolObjectKey;

        [SerializeField] private PoolObjectKey poolObjectKey;
        [SerializeField] private ParticleSystem.MinMaxGradient[] projectileColor;
    }
}