using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DataControl;
using EPOOutline;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TowerControl
{
    public abstract class Tower : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        private MeshRenderer _meshRenderer;
        private MeshFilter _meshFilter;
        private bool _isBuilt;

        protected BoxCollider BoxCollider;

        public event Action<Tower> OnClickTower;
        public float TowerRange { get; private set; }
        public sbyte TowerLevel { get; private set; }
        public Outlinable Outline { get; private set; }
        public TowerData TowerData => towerData;

        [SerializeField] private TowerData towerData;

        #region Unity Event

        protected virtual void Awake()
        {
            Init();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
        }

        public virtual void OnPointerUp(PointerEventData eventData)
        {
            if (!_isBuilt) return;
            if (!Input.GetTouch(0).deltaPosition.Equals(Vector2.zero)) return;
            OnClickTower?.Invoke(this);
            if (Input.touchCount > 1) return;
            Outline.enabled = true;
        }

        #endregion

        #region Abstract Function

        public abstract void TowerTargetInit();
        public abstract void TowerTargeting();
        public abstract UniTaskVoid TowerAttackAsync(CancellationTokenSource cts);

        #endregion

        private void ColliderSize()
        {
            var rendererY = _meshRenderer.bounds.size.y;
            var size = BoxCollider.size;
            size = new Vector3(size.x, rendererY, size.z);
            BoxCollider.size = size;
        }

        protected virtual void Init()
        {
            TowerLevel = -1;
            Outline = GetComponent<Outlinable>();
            Outline.enabled = false;
            BoxCollider = GetComponent<BoxCollider>();
            _meshFilter = transform.GetChild(0).GetComponent<MeshFilter>();
            _meshRenderer = transform.GetChild(0).GetComponent<MeshRenderer>();
        }

        #region Public Function

        public void TowerLevelUp()
        {
            TowerLevel++;
        }

        public virtual void TowerSetting(MeshFilter towerMesh, int damageData, byte rangeData,
            float attackDelayData)
        {
            _isBuilt = true;
            TowerRange = rangeData;
            _meshFilter.sharedMesh = towerMesh.sharedMesh;

            ColliderSize();
        }

        #endregion
    }
}