using ManagerControl;
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
            if (UIManager.instance.enableMoveUnitController) return;
            UIManager.instance.UIOff();
        }
    }
}