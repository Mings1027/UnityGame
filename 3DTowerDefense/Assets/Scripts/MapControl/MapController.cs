using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MapControl
{
    public class MapController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public Transform TowerBuildPoint => towerBuildPoint;
        public Transform[] WayPoints => _wayPoints;
        
        [SerializeField] private Transform towerBuildPoint;
        [SerializeField] private Transform wayPointParent;
        private Transform[] _wayPoints;

        public event Action onCloseUIEvent;

        private void Start()
        {
            _wayPoints = new Transform[wayPointParent.childCount];
            for (var i = 0; i < _wayPoints.Length; i++)
            {
                _wayPoints[i] = wayPointParent.GetChild(i);
            }
        }

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