using UnityEngine;
using UnityEngine.EventSystems;

namespace MapControl
{
    public class ExpandMapButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        private void ExpandMap()
        {
            MapController.Instance.ExpandMap(transform.position);
            gameObject.SetActive(false);
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