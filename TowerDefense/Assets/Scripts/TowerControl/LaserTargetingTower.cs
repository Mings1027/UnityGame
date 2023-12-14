using CustomEnumControl;
using DataControl;
using InterfaceControl;
using UIControl;
using UnityEngine;

namespace TowerControl
{
    public class LaserTargetingTower : TargetingTower, IHit
    {
        private UIManager _uiManager;
        private float _attackMana;
        [SerializeField] private LineRenderer beam;

        protected override void Init()
        {
            base.Init();
            _uiManager = UIManager.Instance;
            var manaTowerData = (ManaTowerData)TowerData;
            _attackMana = manaTowerData.attackMana;
            firePos = transform.GetChild(2);
        }

        public override void TowerTargetInit()
        {
            base.TowerTargetInit();
            beam.enabled = false;
            attackSound.Stop();
        }

        protected override void Detect()
        {
            beam.enabled = false;
            if (attackSound.isPlaying) attackSound.Stop();
            var size = Physics.OverlapSphereNonAlloc(transform.position, TowerRange, targetColliders, targetLayer);
            if (size <= 0)
            {
                target = null;
                isTargeting = false;
                return;
            }

            if (_uiManager.Mana.Current < _attackMana) return;

            var shortestDistance = float.MaxValue;
            for (var i = 0; i < size; i++)
            {
                var distanceToResult =
                    Vector3.SqrMagnitude(transform.position - targetColliders[i].transform.position);
                if (distanceToResult >= shortestDistance) continue;
                shortestDistance = distanceToResult;

                if (targetColliders[i].CompareTag("Flying Monster"))
                {
                    target = targetColliders[i];
                    break;
                }

                target = targetColliders[i];
            }

            isTargeting = true;
            towerState = TowerState.Attack;
        }

        protected override void Attack()
        {
            if (_uiManager.Mana.Current < _attackMana)
            {
                towerState = TowerState.Detect;
                return;
            }

            beam.enabled = true;
            beam.SetPosition(0, firePos.position);
            beam.SetPosition(1, target.bounds.center);
            Hit();
            _uiManager.Mana.Damage(_attackMana);

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