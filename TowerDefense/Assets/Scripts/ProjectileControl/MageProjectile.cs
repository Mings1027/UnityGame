using DataControl;
using ManagerControl;
using UnitControl.EnemyControl;
using UnityEngine;

namespace ProjectileControl
{
    public sealed class MageProjectile : Projectile
    {
        private sbyte _slowDeBuffIndex;
        [SerializeField] private DeBuffData deBuffData;

        public override void ColorInit(sbyte effectIndex)
        {
            base.ColorInit(effectIndex);
            _slowDeBuffIndex = effectIndex;
        }

        public override void Hit()
        {
            base.Hit();
            target.TryGetComponent(out EnemyStatus enemyStatus);
            enemyStatus.SlowEffect(ref deBuffData.speedDeBuffData[_slowDeBuffIndex]);
        }
    }
}