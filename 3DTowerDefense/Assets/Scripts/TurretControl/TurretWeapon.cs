using GameControl;
using UnityEngine;

namespace TurretControl
{
    public class TurretWeapon : MonoBehaviour
    {
        [SerializeField] private Transform[] attackPoint;
        [SerializeField] private string bulletName;

        public void Attack()
        {
            for (int i = 0; i < attackPoint.Length; i++)
            {
                StackObjectPool.Get("CannonVFX", attackPoint[i].position);
                StackObjectPool.Get(bulletName, attackPoint[i].position, attackPoint[i].rotation);
            }
        }
    }
}