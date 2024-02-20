using GameControl;
using ManagerControl;
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

        public byte towerRange { get; private set; }
        public int damage { get; private set; }
        public sbyte towerLevel { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            Init();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            towerLevel = -1;
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
            towerLevel++;
        }

        public virtual void TowerSetting(MeshFilter towerMesh, int damageData, byte rangeData,
            float cooldownData)
        {
            towerRange = rangeData;
            damage = damageData;
            attackCooldown.cooldownTime = cooldownData;
            _meshFilter.sharedMesh = towerMesh.sharedMesh;

            ColliderSize();
        }

        public override void DisableObject()
        {
            UIManager.instance.RemoveAttackTower(this);
            base.DisableObject();
        }

        public abstract void TowerUpdate();
        public abstract void TowerTargetInit();
    }
}