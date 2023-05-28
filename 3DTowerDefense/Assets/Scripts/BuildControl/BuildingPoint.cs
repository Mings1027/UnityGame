using System;
using GameControl;
using UnityEngine;
using UnityEngine.EventSystems;

namespace BuildControl
{
    [DisallowMultipleComponent]
    public class BuildingPoint : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public event Action<Transform> onOpenTowerSelectPanelEvent;

        private void OnDisable()
        {
            StackObjectPool.ReturnToPool(gameObject);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            onOpenTowerSelectPanelEvent?.Invoke(transform);
        }
    }
}