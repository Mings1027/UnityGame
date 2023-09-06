using UnityEngine;
using UnityEngine.EventSystems;

namespace MapControl
{
    public class ExpandMapButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public void ExpandMap()
        {
            MapController.Instance.ExpandMap(transform.position);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            ExpandMap();
        }
    }
}