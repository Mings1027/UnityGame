using System;
using DataControl;
using DG.Tweening;
using GameControl;
using UnityEngine;
using WeaponControl;

namespace TowerControl
{
    public class MageTargetingTower : TargetingTower
    {
        private Sequence _atkSequence;
        private Material _material;
        private MeshFilter _crystalMeshFilter;

        private event Action onAttackEvent;

        [SerializeField] private Transform crystal;
        [SerializeField] private Mesh[] crystalMesh;
        [SerializeField] private Transform[] crystalPositions;
        private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");

        protected override void Awake()
        {
            base.Awake();
            _material = crystal.GetComponent<Renderer>().material;
            _crystalMeshFilter = crystal.GetComponent<MeshFilter>();

            _atkSequence = DOTween.Sequence().SetAutoKill(false).Pause()
                .Append(_material.DOColor(_material.GetColor(EmissionColor) * 2, 0.5f))
                .Append(_material.DOColor(_material.GetColor(EmissionColor), 0.5f));
        }

        public override void TowerInit(MeshFilter consMeshFilter)
        {
            base.TowerInit(consMeshFilter);
            // 타워 업그레이드 중에 crystal 보이면 어색하기 때문에 crystal을 타워밑으로 옮겨 잠시 숨겨줌
            crystal.position = transform.position;
        }

        public override void TowerSetting(MeshFilter towerMeshFilter, int minDamage, int maxDamage, float range,
            float delay, float health = 0)
        {
            base.TowerSetting(towerMeshFilter, minDamage, maxDamage, range, delay, health);
            CrystalPosInit();
            onAttackEvent = null;
            if (IsUniqueTower)
            {
                if (TowerUniqueLevel == 0)
                {
                    onAttackEvent += OrangeAttack;
                }
                else
                {
                    //PurpleAttack 등록해줘야함
                }
            }
            else
            {
                onAttackEvent += NormalAttack;
            }
        }

        private void CrystalPosInit()
        {
            var index = IsUniqueTower ? TowerUniqueLevel + 3 : TowerLevel;
            crystal.position = crystalPositions[index].position;
            _crystalMeshFilter.sharedMesh = crystalMesh[index];
        }

        protected override void Attack()
        {
            _atkSequence.Restart();
            onAttackEvent?.Invoke();
        }

        private void NormalAttack()
        {
            StackObjectPool.Get(PoolObjectName.MageShootSfx, transform.position);
            StackObjectPool.Get<Bullet>(PoolObjectName.BlueMageBullet, crystalPositions[TowerLevel])
                .Init(target, Damage);
        }

        private void OrangeAttack()
        {
            StackObjectPool.Get(PoolObjectName.OrangeMageShootSfx, transform.position);
            StackObjectPool.Get<Bullet>(PoolObjectName.OrangeMageBullet, crystalPositions[TowerUniqueLevel + 3])
                .Init(target, Damage);
        }

        private void PurpleAttack()
        {
            //범위에 들어온 적 순서데로 발사체가 관통하며 공격 전기 이펙트 처럼 하면 좋겟는데 어케 만들지..
        }
    }
}