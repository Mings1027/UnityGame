using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MapControl
{
    public class MapController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public Transform TowerBuildPoint => towerBuildPoint;
        [SerializeField] private Transform towerBuildPoint;

        public event Action onCloseUIEvent;

        public void OnPointerDown(PointerEventData eventData)
        {
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (Input.touchCount <= 0) return;
            if (Input.GetTouch(0).deltaPosition != Vector2.zero) return;
            onCloseUIEvent?.Invoke();
        }
    }
}