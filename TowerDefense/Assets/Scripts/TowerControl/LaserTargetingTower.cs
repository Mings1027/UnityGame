using System.Threading;
using CustomEnumControl;
using InterfaceControl;
using StatusControl;
using UnityEngine;

namespace TowerControl
{
    public class LaserTargetingTower : TargetingTower, IHit
    {
        [SerializeField] private LineRenderer beam;

        protected override void Init()
        {
            base.Init();
            // targetLayer = LayerMask.GetMask("FlyingMonster");
            firePos = transform.GetChild(2);
        }

        public override void TowerTargetInit()
        {
            base.TowerTargetInit();
            beam.enabled = false;
            attackSound.Stop();
        }

        public override void TowerUpdate(CancellationTokenSource cts)
        {
            base.TowerUpdate(cts);
            if (target)
            {
                beam.enabled = true;
                beam.SetPosition(0, firePos.position);
                beam.SetPosition(1, target.bounds.center);
            }
            else
            {
                beam.enabled = false;
                if (attackSound.isPlaying) attackSound.Stop();
            }
        }

        protected override void Patrol()
        {
            var size = Physics.OverlapSphereNonAlloc(transform.position, TowerRange, targetColliders, targetLayer);
            if (size <= 0)
            {
                target = null;
                isTargeting = false;
                return;
            }

            var health = 0f;
            for (var i = 0; i < size; i++)
            {
                var curHealth = targetColliders[i].GetComponent<Health>().Current;
                if (health < curHealth)
                {
                    health = curHealth;
                    target = targetColliders[i];
                }
            }

            isTargeting = true;
            towerState = TowerState.Attack;
        }

        protected override void Attack()
        {
            Hit();
            if (!attackSound.isPlaying)
                attackSound.Play();
        }

        public void Hit()
        {
            if (!target.TryGetComponent(out IDamageable damageable) || !target.enabled) return;
            damageable.Damage(Damage);
        }
    }
}