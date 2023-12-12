using CustomEnumControl;
using GameControl;
using UnityEngine;

namespace DataControl
{
    [CreateAssetMenu(menuName = "Tower Data/Battle Tower Data/Targeting Tower Data")]
    public class TargetingTowerData : BattleTowerData
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