using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace BuildControl
{
    [DisallowMultipleComponent]
    public class BuildingPoint : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
    {
        private Outline _outline;

        public event Action<GameObject,Transform,Quaternion> OnOpenTowerSelectPanelEvent;

        private void Awake()
        {
            _outline = GetComponent<Outline>();
            _outline.enabled = false;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _outline.enabled = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _outline.enabled = false;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            OnOpenTowerSelectPanelEvent?.Invoke(gameObject,transform,transform.rotation);
        }

        private void OnDestroy()
        {
            OnOpenTowerSelectPanelEvent = null;
        }
    }
}