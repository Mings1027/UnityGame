using UnityEngine;
using UnityEngine.EventSystems;

namespace BuildControl
{
    public class BuildingPoint : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
    {
        private Camera _cam;
        private Outline _outline;
        private BuildController _buildController;

        public int index;

        private void Awake()
        {
            _cam = Camera.main;
            _outline = GetComponent<Outline>();
            _buildController = BuildController.Instance;
            _outline.enabled = false;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _outline.enabled = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _outline.enabled = false;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            print(index);
            _buildController.numOfBuildingPoint = index;
            _buildController.OpenBuildPanel(_cam.WorldToScreenPoint(transform.position));
        }
    }
}