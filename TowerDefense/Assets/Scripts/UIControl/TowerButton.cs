using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UIControl
{
    public class TowerButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        public event Action<int, Vector3> OnClick;
        public byte buttonIndex { get; set; }

        public void OnPointerDown(PointerEventData eventData)
        {
            OnClick?.Invoke(buttonIndex, transform.position);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            UIManager.Instance.towerCardUI.DisableCard().Forget();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            UIManager.Instance.towerCardUI.DisableCard().Forget();
        }
    }
}