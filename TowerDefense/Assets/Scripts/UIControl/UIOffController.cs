using ManagerControl;
using UnitControl;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UIControl
{
    public class UIOffController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public void OnPointerDown(PointerEventData eventData)
        {
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (Input.GetTouch(0).deltaPosition != Vector2.zero) return;
            if (MoveUnitController.enableMoveUnitController) return;
            BuildTowerManager.DeSelectTower();
            UIManager.AppearUI();
        }
    }
}