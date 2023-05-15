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
        private Transform[] _crystalPositions;
        private Transform crystal;

        [SerializeField] private Mesh[] crystalMesh;
        private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");

        protected override void Awake()
        {
            base.Awake();
            crystal = GameObject.Find("Crystal").transform;
            material = crystal.GetComponent<Renderer>().material;
            _crystalMeshFilter = crystal.GetComponent<MeshFilter>();

            var crystalPosition = GameObject.Find("CrystalPosition").transform;
            _crystalPositions = new Transform[crystalPosition.childCount];
            for (var i = 0; i < _crystalPositions.Length; i++)
            {
                _crystalPositions[i] = crystalPosition.GetChild(i);
            }

            atkSequence = DOTween.Sequence().SetAutoKill(false).Pause()
                .Append(material.DOColor(material.GetColor(EmissionColor) * 2, 0.5f))
                .Append(material.DOColor(material.GetColor(EmissionColor), 0.5f));
        }

        public override void TowerInit(MeshFilter consMeshFilter,int towerCoin)
        {
            base.TowerInit(consMeshFilter,towerCoin);
            crystal.position = transform.position;
        }

        public override void TowerSetting(MeshFilter towerMeshFilter, int minDamage, int maxDamage, float range,
            float delay)
        {
            base.TowerSetting(towerMeshFilter, minDamage, maxDamage, range, delay);
            crystal.position = _crystalPositions[TowerLevel].position;
            _crystalMeshFilter.sharedMesh = crystalMesh[TowerLevel];
        }

        protected override void Attack()
        {
            atkSequence.Restart();
            StackObjectPool
                .Get<Bullet>("MageBullet", _crystalPositions[TowerLevel].position + new Vector3(0, 3, 0))
                .Init(target, Damage);
        }
    }
}