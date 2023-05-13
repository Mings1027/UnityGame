using System;
using ManagerControl;
using UIControl;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UnitControl
{
    public class MoveUnitIndicator : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        private UIManager _uiManager;

        private void Awake()
        {
            _uiManager = UIManager.Instance;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _uiManager.MoveUnit();
            gameObject.SetActive(false);
        }
    }
}