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
            float delay)
        {
            base.TowerSetting(towerMeshFilter, minDamage, maxDamage, range, delay);
            CrystalPosInit();
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
            StackObjectPool.Get<Bullet>("MageBullet", crystalPositions[TowerLevel].position + new Vector3(0, 3, 0))
                .Init(target, Damage);
        }
    }
}