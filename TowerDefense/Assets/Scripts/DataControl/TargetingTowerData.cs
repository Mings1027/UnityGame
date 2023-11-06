using UnityEngine;

namespace DataControl
{
    [CreateAssetMenu]
    public class TargetingTowerData : TowerData
    {
        public ParticleSystem.MinMaxGradient[] ProjectileColor => projectileColor;
        [SerializeField] private ParticleSystem.MinMaxGradient[] projectileColor;
    }
}