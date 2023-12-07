using UnityEngine;
using UnityEngine.EventSystems;

namespace UnitControl.TowerUnitControl
{
    public class MoveTowerUnit : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        private TowerUnit _towerUnit;

        private void Awake()
        {
            _towerUnit = GetComponent<TowerUnit>();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (Input.touchCount > 1) return;
            if (!Input.GetTouch(0).deltaPosition.Equals(Vector2.zero)) return;
            _towerUnit.ActiveIndicator();
        }
    }
}