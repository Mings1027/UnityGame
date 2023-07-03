using UnitControl.EnemyControl;
using UnityEngine;

namespace WeaponControl
{
    public class SlowDownMageBullet : MageBullet
    {
        [SerializeField] private float deBuffTime;
        [SerializeField] private float decreaseSpeed;

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