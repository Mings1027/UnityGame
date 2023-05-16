using System;
using ManagerControl;
using UIControl;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UnitControl
{
    public class MoveUnitIndicator : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public event Action onMoveUnitEvent;

        public void OnPointerDown(PointerEventData eventData)
        {
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            onMoveUnitEvent?.Invoke();
            gameObject.SetActive(false);
        }
    }
}