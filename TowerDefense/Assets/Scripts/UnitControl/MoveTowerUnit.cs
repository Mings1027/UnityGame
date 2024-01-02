using TowerControl;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UnitControl
{
    [DisallowMultipleComponent]
    public class MoveTowerUnit : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        private TowerUnit _towerUnit;
        private SummonTower _summonTower;

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