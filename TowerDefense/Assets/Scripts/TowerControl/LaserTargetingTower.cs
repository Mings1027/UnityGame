using CustomEnumControl;
using DataControl;
using DataControl.TowerData;
using InterfaceControl;
using ManagerControl;
using UnityEngine;

namespace TowerControl
{
    public class LaserTargetingTower : TargetingTower
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
            var size = Physics.OverlapSphereNonAlloc(transform.position, TowerRange, targetColliders, targetLayer);
            if (size <= 0)
            {
                target = null;
                isTargeting = false;
                return;
            }

            if (towerMana.towerMana.Current < _attackMana) return;

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
            if (!target || !target.enabled || towerMana.towerMana.Current < _attackMana)
            {
                towerState = TowerState.Detect;
                // if (attackSound.isPlaying) attackSound.Stop();
                beam.enabled = false;
                beam.SetPosition(0, firePos.position);
                beam.SetPosition(1, firePos.position);
                return;
            }

            beam.SetPosition(0, firePos.position);
            beam.SetPosition(1, target.bounds.center);

            if (attackCooldown.IsCoolingDown) return;
            // if (!attackSound.isPlaying)
            //     attackSound.Play();
            Attack();
            attackCooldown.StartCooldown();
        }

        protected override void Attack()
        {
            Hit();
            SoundManager.Instance.Play3DSound(audioClip, transform.position);
            towerMana.towerMana.Damage(_attackMana);
        }

        public void Hit()
        {
            if (!target.TryGetComponent(out IDamageable damageable) || !target.enabled) return;
            damageable.Damage(_isFlyingMonster ? Damage : Damage * 0.5f);
        }
    }
}