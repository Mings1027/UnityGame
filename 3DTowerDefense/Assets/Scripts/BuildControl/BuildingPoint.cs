using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace BuildControl
{
    [DisallowMultipleComponent]
    public class BuildingPoint : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
    {
        private Camera _cam;
        private Outline _outline;
        private BuildCanvasController _buildCanvasController;

        public int index;
        private void Awake()
        {
            _cam = Camera.main;
            _outline = GetComponent<Outline>();
            _buildCanvasController = BuildCanvasController.Instance as BuildCanvasController;
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
            _buildCanvasController.OpenBuildPanel(index, transform.position, transform.rotation,
                _cam.WorldToScreenPoint(transform.position));
        }
    }
}