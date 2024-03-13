using InterfaceControl;

namespace MonsterControl
{
    public class GroundBossUnit : GroundMonster
    {
        protected override void TryDamage()
        {
            for (var i = 0; i < targetCollider.Length; i++)
            {
                if (targetCollider[i] && targetCollider[i].enabled &&
                    targetCollider[i].TryGetComponent(out IDamageable damageable))
                {
                    damageable.Damage(monsterData.damage);
                }
            }
        }
    }
}