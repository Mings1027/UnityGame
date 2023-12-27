using System;
using CustomEnumControl;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TowerControl
{
    [DisallowMultipleComponent]
    public abstract class Tower : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public TowerType TowerType => towerType;
        public event Action<Tower> OnClickTowerAction;

        [SerializeField] private TowerType towerType;

        #region Unity Event

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

        public virtual void DisableObject()
        {
            var g = gameObject;
            g.SetActive(false);
            Destroy(g);
        }

        #endregion
    }
}