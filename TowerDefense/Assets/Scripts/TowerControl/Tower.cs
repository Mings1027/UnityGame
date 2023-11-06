using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DataControl;
using EPOOutline;
using InterfaceControl;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TowerControl
{
    public abstract class Tower : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        private Outlinable outline;
        private MeshRenderer _meshRenderer;
        private MeshFilter _meshFilter;

        protected BoxCollider boxCollider;

        public event Action<Tower> OnClickTower;
        public float TowerRange { get; private set; }
        public sbyte TowerLevel { get; private set; }
        public int TowerInvestment { get; set; }
        public TowerData TowerData => towerData;

        [SerializeField] private TowerData towerData;

        protected virtual void Awake()
        {
            Init();
        }

        //==================================Custom Method====================================================
        //======================================================================================================
        public abstract void TowerTargetInit();
        public abstract void TowerTargeting();
        public abstract UniTaskVoid TowerAttackAsync(CancellationTokenSource cts);

        protected virtual void Init()
        {
            TowerLevel = -1;
            outline = GetComponent<Outlinable>();
            outline.enabled = false;
            boxCollider = GetComponent<BoxCollider>();
            _meshFilter = transform.GetChild(0).GetComponent<MeshFilter>();
            _meshRenderer = transform.GetChild(0).GetComponent<MeshRenderer>();
        }

        public void TowerLevelUp()
        {
            TowerLevel++;
        }

        public virtual void TowerSetting(MeshFilter towerMesh, int damageData, byte rangeData,
            float attackDelayData)
        {
            TowerRange = rangeData;
            _meshFilter.sharedMesh = towerMesh.sharedMesh;

            ColliderSize();
        }

        private void ColliderSize()
        {
            var rendererY = _meshRenderer.bounds.size.y;
            var size = boxCollider.size;
            size = new Vector3(size.x, rendererY, size.z);
            boxCollider.size = size;
        }

        public void DisableOutline() => outline.enabled = false;

        public void OnPointerDown(PointerEventData eventData)
        {
        }

        public virtual void OnPointerUp(PointerEventData eventData)
        {
            if (!Input.GetTouch(0).deltaPosition.Equals(Vector2.zero)) return;
            OnClickTower?.Invoke(this);
            if (Input.touchCount > 1) return;
            outline.enabled = true;
        }
    }
}