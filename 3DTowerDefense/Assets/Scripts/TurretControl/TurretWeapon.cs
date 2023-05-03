using BulletControl;
using GameControl;
using UnityEngine;

namespace TurretControl
{
    public class TurretWeapon : MonoBehaviour
    {
        [SerializeField] private Transform[] attackPoint;
        [SerializeField] private string bulletName;

        public void Attack(int damage, Transform target)
        {
            for (int i = 0; i < attackPoint.Length; i++)
            {
                var pointPos = attackPoint[i].position;
                var pointRot = attackPoint[i].rotation;
                StackObjectPool.Get("BulletExplosionVFX", pointPos);
                StackObjectPool.Get("BulletShootVFX", pointPos, pointRot);
                StackObjectPool.Get<Bullet>(bulletName, pointPos, pointRot).Init(damage, target);
            }
        }
    }
}