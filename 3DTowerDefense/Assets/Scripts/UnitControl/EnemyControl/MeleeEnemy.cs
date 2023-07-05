using GameControl;

namespace UnitControl.EnemyControl
{
    public class MeleeEnemy : EnemyUnit
    {
        protected override void Attack()
        {
            if (Target.TryGetComponent(out Health h))
            {
                h.TakeDamage(Damage);
            }
        }
    }
}