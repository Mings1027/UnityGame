using System;
using Cysharp.Threading.Tasks;
using GameControl;
using UnityEngine;
using WeaponControl;

namespace EnemyControl
{
    public class ArcherEnemy : Enemy
    {
        protected override void Attack()
        {
            SpawnArrow(target.position);
        }

        private void SpawnArrow(Vector3 t)
        {
            var p = StackObjectPool.Get<Projectile>("ArcherArrow", transform.position, Quaternion.Euler(-90, 0, 0));
            p.Parabola(transform, t).Forget();
            p.damage = damage;
        }

        protected override async UniTaskVoid StartCoolDown()
        {
            attackAble = false;
            await UniTask.Delay(TimeSpan.FromSeconds(AtkDelay), cancellationToken: cts.Token);
            attackAble = true;
        }
    }
}