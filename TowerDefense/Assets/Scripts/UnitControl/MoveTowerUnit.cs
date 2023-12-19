using TowerControl;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UnitControl
{
    [DisallowMultipleComponent]
    public class MoveTowerUnit : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        private TowerUnit _towerUnit;
        private UnitTower _unitTower;

        private void Awake()
        {
            _towerUnit = GetComponentInParent<TowerUnit>();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _towerUnit.ParentPointerUp();
        }
    }
}