using DataControl;
using GameControl;
using UnitControl.EnemyControl;
using UnityEngine;

namespace ProjectileControl
{
    public class MageBullet : Bullet
    {
        [SerializeField] private float deBuffTime;
        [SerializeField] private int decreaseSpeed;

        protected override void BulletHit(Component other)
        {
            if (other.TryGetComponent(out EnemyUnit e))
            {
                e.SlowMovement(deBuffTime, decreaseSpeed).Forget();
            }

            base.BulletHit(other);
        }
    }
}