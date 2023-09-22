using DataControl;
using ManagerControl;
using UnitControl.EnemyControl;
using UnityEngine;

namespace ProjectileControl
{
    public sealed class MageProjectile : Projectile
    {
        private int _index;
        [SerializeField] private DeBuffData deBuffData;

        protected override void Awake()
        {
            base.Awake();
            towerType = TowerType.Mage;
        }

        public override void Hit()
        {
            base.Hit();
            target.TryGetComponent(out EnemyStatus enemyStatus);
            enemyStatus.SlowEffect(ref deBuffData.speedDeBuffData[_index]);
        }

        public void DeBuffInit(int index)
        {
            _index = index;
        }
    }
}