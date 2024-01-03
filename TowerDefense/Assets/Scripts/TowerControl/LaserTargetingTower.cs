using CustomEnumControl;
using DataControl.TowerDataControl;
using InterfaceControl;
using ManagerControl;
using UnityEngine;

namespace TowerControl
{
    public class LaserTargetingTower : ContinuousDamageTower
    {
        private float _attackMana;
        private bool _isFlyingMonster;
        [SerializeField] private LineRenderer beam;
        [SerializeField] private TowerMana towerMana;

        protected override void Init()
        {
            base.Init();
            var manaTowerData = (ManaUsingTowerData)UIManager.Instance.TowerDataPrefabDictionary[TowerType].towerData;
            _attackMana = manaTowerData.attackMana;
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
            if (towerMana.towerMana.Current < _attackMana) return;
            var size = Physics.OverlapSphereNonAlloc(transform.position, TowerRange, targetColliders, targetLayer);
            if (size <= 0)
            {
                target = null;
                isTargeting = false;
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
                if (targetColliders[i].CompareTag("Flying Monster"))
                {
                    _isFlyingMonster = true;
                    target = targetColliders[i];
                    break;
                }
            }

            _isFlyingMonster = false;
            isTargeting = true;
            beam.enabled = true;
            towerState = TowerState.Attack;
            patrolCooldown.StartCooldown();
        }

        protected override void ReadyToAttack()
        {
            if (!target || !target.enabled || towerMana.towerMana.Current < _attackMana)
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
            SoundManager.Instance.Play3DSound(audioClip, transform.position);
            towerMana.towerMana.Damage(_attackMana);
        }

        protected override void Hit()
        {
            if (!target.TryGetComponent(out IDamageable damageable) || !target.enabled) return;
            damageable.Damage(_isFlyingMonster ? Damage : Damage * 0.5f);
        }
    }
}