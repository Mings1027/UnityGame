using CustomEnumControl;
using UnityEngine;
using Utilities;

namespace DataControl.TowerDataControl
{
    [CreateAssetMenu(menuName = "Tower Data/Attack Tower Data/Targeting Tower Data")]
    public class TargetingTowerData : AttackTowerData
    {
        public ParticleSystem.MinMaxGradient[] ProjectileColor => projectileColor;
        public PoolObjectKey PoolObjectKey => poolObjectKey;

        [SerializeField] private PoolObjectKey poolObjectKey;
        [SerializeField] private ParticleSystem.MinMaxGradient[] projectileColor;
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
        }
    }
}