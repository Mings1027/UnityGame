using InterfaceControl;

namespace UnitControl.EnemyControl
{
    public class BossUnit : MonsterUnit
    {
        protected override void DisableObject()
        {
            base.DisableObject();
            Destroy(gameObject);
        }

        protected override void TryDamage()
        {
            for (var i = 0; i < targetCollider.Length; i++)
            {
                if (targetCollider[i] && targetCollider[i].enabled &&
                    targetCollider[i].TryGetComponent(out IDamageable damageable))
                {
                    damageable.Damage(damage);
                }
            }
        }
    }
}