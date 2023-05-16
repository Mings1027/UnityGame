using System;
using GameControl;
using UnityEngine.EventSystems;

namespace UIControl
{
    public class MapController : Singleton<MapController>, IPointerDownHandler, IPointerUpHandler
    {
        public event Action onCloseUIEvent; 

        public void OnPointerDown(PointerEventData eventData)
        {
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            onCloseUIEvent?.Invoke();
        }
    }
}