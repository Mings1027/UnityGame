using System;
using CustomEnumControl;
using ManagerControl;
using PoolObjectControl;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MapControl
{
    public class ExpandMapButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public event Action<Vector3> OnExpandMapEvent;

        public void Expand()
        {
            OnExpandMapEvent?.Invoke(transform.position);
        }

        private void OnDisable()
        {
            OnExpandMapEvent = null;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (Input.touchCount > 1) return;
            if (!Input.GetTouch(0).deltaPosition.Equals(Vector2.zero)) return;
            if (UIManager.enableMoveUnitController) return;
            var position = transform.position;
            // PoolObjectManager.Get(PoolObjectKey.ExpandMapSmoke, position);
            OnExpandMapEvent?.Invoke(transform.position);
        }
    }
}