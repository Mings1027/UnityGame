using System;
using CustomEnumControl;
using EPOOutline;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TowerControl
{
    [DisallowMultipleComponent]
    public abstract class Tower : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        private Outlinable _towerOutline;

        public event Action<Tower> OnClickTowerAction;

        [field: SerializeField] public TowerType towerType { get; private set; }

#region Unity Event

        protected virtual void Awake()
        {
            _towerOutline = GetComponent<Outlinable>();
            _towerOutline.enabled = false;
        }

        protected virtual void OnEnable()
        {
        }

        public void OnPointerDown(PointerEventData eventData)
        {
        }

        public virtual void OnPointerUp(PointerEventData eventData)
        {
            if (Input.touchCount != 1) return;
            if (!Input.GetTouch(0).deltaPosition.Equals(Vector2.zero)) return;
            OnClickTowerAction?.Invoke(this);
        }

#endregion

#region Public Function

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
            var g = gameObject;
            g.SetActive(false);
            Destroy(g);
        }

#endregion
    }
}