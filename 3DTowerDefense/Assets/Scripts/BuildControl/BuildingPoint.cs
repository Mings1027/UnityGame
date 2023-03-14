using System;
using GameControl;
using UnityEngine;
using UnityEngine.EventSystems;

namespace BuildControl
{
    [DisallowMultipleComponent]
    public class BuildingPoint : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private Outline _outline;

        // public event Action<Transform> onOpenTowerSelectPanelEvent;

        private void Awake()
        {
            _outline = GetComponent<Outline>();
            _outline.enabled = false;
        }

        private void OnDisable()
        {
            StackObjectPool.ReturnToPool(gameObject);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _outline.enabled = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _outline.enabled = false;
        }

        // public void OnPointerDown(PointerEventData eventData)
        // {
        //     // onOpenTowerSelectPanelEvent?.Invoke(transform);
        // }
        //
        // private void OnDestroy()
        // {
        //     onOpenTowerSelectPanelEvent = null;
        // }
    }
}