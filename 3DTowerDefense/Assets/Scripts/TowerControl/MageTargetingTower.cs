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
        private Sequence atkSequence;
        private Material material;
        private MeshFilter _crystalMeshFilter;

        private event Action onAttackEvent;

        [SerializeField] private Transform crystal;
        [SerializeField] private Mesh[] crystalMesh;
        [SerializeField] private Transform[] crystalPositions;
        private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");

        protected override void Awake()
        {
            base.Awake();
            material = crystal.GetComponent<Renderer>().material;
            _crystalMeshFilter = crystal.GetComponent<MeshFilter>();

            atkSequence = DOTween.Sequence().SetAutoKill(false).Pause()
                .Append(material.DOColor(material.GetColor(EmissionColor) * 2, 0.5f))
                .Append(material.DOColor(material.GetColor(EmissionColor), 0.5f));
        }

        public override void TowerInit(MeshFilter consMeshFilter)
        {
            base.TowerInit(consMeshFilter);
            // 타워 업그레이드 중에 crystal 보이면 어색하기 때문에 crystal을 타워밑으로 옮겨 잠시 숨겨줌
            crystal.position = transform.position;
        }

        public override void TowerSetting(MeshFilter towerMeshFilter, int minDamage, int maxDamage, float range,
            float delay, int health = 0)
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
                    //보라색 타워 전기처럼 
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
            atkSequence.Restart();
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
            //오렌지색 공격 소리 넣어야함
            StackObjectPool.Get<Bullet>(PoolObjectName.OrangeMageBullet, crystalPositions[TowerUniqueLevel + 3])
                .Init(target, Damage);
        }
    }
}