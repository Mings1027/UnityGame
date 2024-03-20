using CustomEnumControl;
using DataControl;
using DataControl.TowerDataControl;
using InterfaceControl;
using ManagerControl;
using MonsterControl;
using StatusControl;
using UIControl;
using UnityEngine;

namespace TowerControl
{
    public class LaserTargetingTower : ContinuousDamageTower
    {
        private byte _attackMana;
        private bool _isFlyingMonster;
        private Mana _towerMana;
        [SerializeField] private LineRenderer beam;

        protected override void Init()
        {
            base.Init();
            var manaTowerData = (ManaUsingTowerData)UIManager.towerDataDic[towerType];
            _attackMana = manaTowerData.initMana;
            _towerMana = GameHUD.towerMana;
            firePos = transform.GetChild(2);
        }

        public override void TowerTargetInit()
        {
            base.TowerTargetInit();
            beam.enabled = false;
        }

        protected override void Detect()
        {
            if (patrolCooldown.IsCoolingDown) return;
            if (_towerMana.Current < _attackMana) return;
            var size = Physics.OverlapSphereNonAlloc(transform.position, towerRange, targetColliders, targetLayer);
            if (size <= 0)
            {
                target = null;
                return;
            }

            var shortestDistance = float.MaxValue;
            for (var i = 0; i < size; i++)
            {
                var distanceToResult = Vector3.SqrMagnitude(transform.position - targetColliders[i].bounds.center);
                if (shortestDistance > distanceToResult)
                {
                    shortestDistance = distanceToResult;
                    target = targetColliders[i];
                }
            }

            for (var i = 0; i < size; i++)
            {
                if (targetColliders[i].TryGetComponent(out FlyingMonster _))
                {
                    _isFlyingMonster = true;
                    target = targetColliders[i];
                    break;
                }
            }

            _isFlyingMonster = false;
            beam.enabled = true;
            towerState = TowerState.Attack;
            patrolCooldown.StartCooldown();
        }

        protected override void ReadyToAttack()
        {
            if (!target || !target.enabled || _towerMana.Current < _attackMana)
            {
                towerState = TowerState.Detect;
                beam.enabled = false;
                beam.SetPosition(0, firePos.position);
                beam.SetPosition(1, firePos.position);
                return;
            }

            if (attackCooldown.IsCoolingDown) return;

            beam.SetPosition(0, firePos.position);
            beam.SetPosition(1, target.bounds.center);

            Attack();
            attackCooldown.StartCooldown();
        }

        protected override void Attack()
        {
            Hit();
            SoundManager.Play3DSound(audioClip, transform.position);
            _towerMana.Damage(_attackMana);
        }

        protected override void Hit()
        {
            if (!target.TryGetComponent(out IDamageable damageable) || !target.enabled) return;
            damageable.Damage((int)(_isFlyingMonster ? towerDamage : towerDamage * 0.5f));
        }
    }
}