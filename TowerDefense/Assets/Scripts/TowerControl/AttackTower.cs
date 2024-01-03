using GameControl;
using UnityEngine;

namespace TowerControl
{
    public abstract class AttackTower : Tower
    {
        private MeshRenderer _meshRenderer;
        private MeshFilter _defaultMesh;
        private MeshFilter _meshFilter;
        
        protected BoxCollider boxCollider;
        protected Cooldown attackCooldown;
        protected Cooldown patrolCooldown;

        public byte TowerRange { get; private set; }
        public int Damage { get; private set; }
        public sbyte TowerLevel { get; private set; }

        protected virtual void Awake()
        {
            Init();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            TowerLevel = -1;
            _meshFilter = _defaultMesh;
        }

        private void ColliderSize()
        {
            var rendererY = _meshRenderer.bounds.size.y;
            var size = boxCollider.size;
            size = new Vector3(size.x, rendererY, size.z);
            boxCollider.size = size;
        }

        protected virtual void Init()
        {
            boxCollider = GetComponent<BoxCollider>();
            _meshFilter = transform.GetChild(0).GetComponent<MeshFilter>();
            _meshRenderer = transform.GetChild(0).GetComponent<MeshRenderer>();
            _defaultMesh = _meshFilter;
        }

        public void TowerLevelUp()
        {
            TowerLevel++;
        }

        public virtual void TowerSetting(MeshFilter towerMesh, int damageData, byte rangeData,
            ushort rpmData)
        {
            TowerRange = rangeData;
            Damage = damageData;
            attackCooldown.cooldownTime = 60 / (float)rpmData;
            _meshFilter.sharedMesh = towerMesh.sharedMesh;

            ColliderSize();
        }

        public abstract void TowerUpdate();
        public abstract void TowerTargetInit();
    }
}