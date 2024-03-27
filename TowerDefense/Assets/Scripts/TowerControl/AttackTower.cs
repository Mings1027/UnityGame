using GameControl;
using ManagerControl;
using UnityEngine;
using Utilities;

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

        public int towerDamage { get; private set; }

        [SerializeField] protected LayerMask targetLayer;

        protected override void Awake()
        {
            base.Awake();
            Init();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            _meshFilter = _defaultMesh;
        }

        private void ColliderSize()
        {
            var rendererY = _meshRenderer.bounds.size.y;
            var size = boxCollider.size;
            size = new Vector3(size.x, rendererY, size.z);
            boxCollider.size = size;
        }

        public override void Init()
        {
            base.Init();
            boxCollider = GetComponent<BoxCollider>();
            _meshFilter = transform.GetChild(0).GetComponent<MeshFilter>();
            _meshRenderer = transform.GetChild(0).GetComponent<MeshRenderer>();
            _defaultMesh = _meshFilter;
            attackCooldown = new Cooldown();
            patrolCooldown = new Cooldown();
        }

        public virtual void TowerSetting(int damageData, float cooldownData, MeshFilter towerMesh)
        {
            towerDamage = damageData;
            attackCooldown.cooldownTime = cooldownData;
            _meshFilter.sharedMesh = towerMesh.sharedMesh;

            ColliderSize();
        }
    }
}