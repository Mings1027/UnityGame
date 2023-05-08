using GameControl;

namespace TurretControl
{
    public class RangeTurret : Turret
    {
        protected override void Targeting()
        {
            if (AttackAble)
            {
                Attack();
                StartCoolDown();
            }
        }

        protected override void Attack()
        {
            
        }
    }
}