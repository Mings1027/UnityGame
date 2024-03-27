using System;
using DataControl.TowerDataControl;
using EPOOutline;
using InterfaceControl;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TowerControl
{
    [DisallowMultipleComponent]
    public abstract class Tower : MonoBehaviour, ITower, IPointerDownHandler, IPointerUpHandler
    {
        private Outlinable _towerOutline;

        protected TowerData towerData;

        public byte towerLevel { get; private set; }
        public event Action<Tower, TowerData> OnSelectTowerEvent;
        public event Action<Tower> OnRemoveEvent;

#region Unity Event

        protected virtual void Awake()
        {
            _towerOutline = GetComponent<Outlinable>();
            _towerOutline.enabled = false;
        }

        protected virtual void OnEnable()
        {
            towerLevel = 0;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
        }

        public virtual void OnPointerUp(PointerEventData eventData)
        {
            if (Input.touchCount != 1) return;
            if (!Input.GetTouch(0).deltaPosition.Equals(Vector2.zero)) return;
            OnSelectTowerEvent?.Invoke(this, towerData);
        }

#endregion

#region Public Function

        public virtual void SetTowerData(TowerData towerData)
        {
            this.towerData = towerData;
        }

        public virtual void ActiveIndicator()
        {
            _towerOutline.enabled = true;
        }

        public virtual void DeActiveIndicator()
        {
            _towerOutline.enabled = false;
        }

        public virtual void DisableObject()
        {
            OnRemoveEvent?.Invoke(this);
            var g = gameObject;
            g.SetActive(false);
            Destroy(g);
        }

#endregion

        public virtual void Init()
        {
        }

        public virtual void LevelUp()
        {
            towerLevel++;
        }

        public virtual void TowerUpdate()
        {
        }

        public virtual void TowerPause()
        {
        }
    }
}