using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DataControl;
using InterfaceControl;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TowerControl
{
    public abstract class Tower : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        protected BoxCollider boxCollider;
        private MeshRenderer _meshRenderer;
        private MeshFilter _meshFilter;

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

        public void OnPointerDown(PointerEventData eventData)
        {
        }

        public virtual void OnPointerUp(PointerEventData eventData)
        {
            OnClickTower?.Invoke(this);
        }
    }
}