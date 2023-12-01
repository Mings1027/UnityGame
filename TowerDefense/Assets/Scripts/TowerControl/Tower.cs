using System;
using System.Threading;
using DataControl;
using EPOOutline;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TowerControl
{
    public abstract class Tower : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        private MeshRenderer _meshRenderer;
        private MeshFilter _defaultMesh;
        private MeshFilter _meshFilter;
        private bool _isBuilt;

        protected BoxCollider boxCollider;
        protected float AttackDelay { get; private set; }

        public event Action<Tower> OnClickTower;
        public byte TowerRange { get; private set; }
        public int Damage { get; private set; }
        public sbyte TowerLevel { get; private set; }

        public Outlinable Outline { get; private set; }
        public TowerData TowerData => towerData;

        [SerializeField] private TowerData towerData;

        #region Unity Event

        protected virtual void Awake()
        {
            Init();
        }

        protected virtual void OnEnable()
        {
            TowerLevel = -1;
            Outline.enabled = false;
            _meshFilter = _defaultMesh;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
        }

        public virtual void OnPointerUp(PointerEventData eventData)
        {
            if (!_isBuilt) return;
            if (Input.touchCount > 1) return;
            if (!Input.GetTouch(0).deltaPosition.Equals(Vector2.zero)) return;
            OnClickTower?.Invoke(this);
            Outline.enabled = true;
        }

        #endregion

        #region Abstract Function

        public abstract void TowerUpdate(CancellationTokenSource cts);
        // public abstract void TowerAttack(CancellationTokenSource cts);
        public abstract void TowerTargetInit();

        #endregion

        private void ColliderSize()
        {
            var rendererY = _meshRenderer.bounds.size.y;
            var size = boxCollider.size;
            size = new Vector3(size.x, rendererY, size.z);
            boxCollider.size = size;
        }

        protected virtual void Init()
        {
            Outline = GetComponent<Outlinable>();
            boxCollider = GetComponent<BoxCollider>();
            _meshFilter = transform.GetChild(0).GetComponent<MeshFilter>();
            _meshRenderer = transform.GetChild(0).GetComponent<MeshRenderer>();
            _defaultMesh = _meshFilter;
        }

        #region Public Function

        public void TowerLevelUp()
        {
            TowerLevel++;
        }

        public virtual void TowerSetting(MeshFilter towerMesh, int damageData, byte rangeData,
            ushort rpmData)
        {
            _isBuilt = true;
            TowerRange = rangeData;
            Damage = damageData;
            AttackDelay = 60 / (float)rpmData;
            _meshFilter.sharedMesh = towerMesh.sharedMesh;

            ColliderSize();
        }

        #endregion
    }
}