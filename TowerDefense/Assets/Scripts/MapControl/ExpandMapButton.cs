using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MapControl
{
    public class ExpandMapButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public event Action<Vector3> OnExpandMapEvent;

        public void OnPointerDown(PointerEventData eventData)
        {
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            OnExpandMapEvent?.Invoke(transform.position);
        }

        public void Expand()
        {
            OnExpandMapEvent?.Invoke(transform.position);
        }

        private void OnDisable()
        {
            OnExpandMapEvent = null;
        }
    }
}