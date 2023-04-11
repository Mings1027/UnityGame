using UnityEngine;

namespace WeaponControl
{
    public class ArrowBullet : Projectile
    {
        public override void Init(Transform t, int damage)
        {
            _endPos = t.position;
            print(_endPos);
            _damage = damage;
        }
    }
}