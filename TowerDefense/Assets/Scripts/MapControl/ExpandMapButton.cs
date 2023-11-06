using System;
using CustomEnumControl;
using InterfaceControl;
using ManagerControl;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MapControl
{
    public class ExpandMapButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public event Action<Vector3> OnExpandMapEvent;

        public void Expand()
        {
            OnExpandMapEvent?.Invoke(transform.position);
        }

        private void OnDisable()
        {
            OnExpandMapEvent = null;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!Input.GetTouch(0).deltaPosition.Equals(Vector2.zero)) return;
            OnExpandMapEvent?.Invoke(transform.position);
            SoundManager.Instance.PlayBGM(SoundEnum.WaveStart);
        }
    }
}