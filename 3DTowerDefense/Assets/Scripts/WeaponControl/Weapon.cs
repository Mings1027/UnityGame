using BulletControl;
using GameControl;
using UnityEngine;

namespace WeaponControl
{
    public abstract class Weapon : MonoBehaviour
    {
        private Transform atkPos;

        [SerializeField] protected string bulletType;

        protected virtual void Awake()
        {
            atkPos = transform.GetChild(0);
        }

        public void Attack(int damage, Vector3 dir)
        {
            var pos = atkPos.position;
            var rot = atkPos.rotation;
            StackObjectPool.Get("BulletExplosionVFX", pos);
            StackObjectPool.Get("BulletShootVFX", pos, rot);
            StackObjectPool.Get<Bullet>(bulletType, pos, rot).Init(damage, dir);
        }
    }
}