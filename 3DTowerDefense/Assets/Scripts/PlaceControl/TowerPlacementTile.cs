using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace PlaceControl
{
    public class TowerPlacementTile : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public Outline Outline { get; private set; }

        public event Action<TowerPlacementTile, Transform> onOpenTowerPanelEvent;

        private void Awake()
        {
            Outline = GetComponent<Outline>();
            Outline.enabled = false;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            Outline.enabled = true;
            onOpenTowerPanelEvent?.Invoke(this, transform);
        }
    }
}