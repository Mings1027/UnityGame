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
        private bool _isFlyingMonster;
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
                    _isFlyingMonster = true;
                    target = targetColliders[i];
                    break;
                }

                _isFlyingMonster = false;
                target = targetColliders[i];
            }

            isTargeting = true;
            towerState = TowerState.Attack;
            beam.enabled = true;
        }

        protected override void ReadyToAttack()
        {
            if (!target || !target.enabled || _uiManager.Mana.Current < _attackMana)
            {
                towerState = TowerState.Detect;
                if (attackSound.isPlaying) attackSound.Stop();
                beam.enabled = false;
                beam.SetPosition(0, firePos.position);
                beam.SetPosition(1, firePos.position);
                return;
            }

            beam.SetPosition(0, firePos.position);
            beam.SetPosition(1, target.bounds.center);

            if (attackCooldown.IsCoolingDown) return;
            if (!attackSound.isPlaying)
                attackSound.Play();
            Attack();
            attackCooldown.StartCooldown();
        }

        protected override void Attack()
        {
            Hit();
            _uiManager.Mana.Damage(_attackMana);
        }

        public void Hit()
        {
            if (!target.TryGetComponent(out IDamageable damageable) || !target.enabled) return;
            damageable.Damage(_isFlyingMonster ? Damage : Damage * 0.5f);
        }
    }
}