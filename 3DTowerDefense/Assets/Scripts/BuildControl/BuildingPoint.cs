using UnityEngine;
using UnityEngine.EventSystems;

namespace BuildControl
{
    public class BuildingPoint : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
    {
        // private Camera _cam;
        private Outline _outline;
        private BuildController _towerController;
        private Vector3 _buildPos;
        private Quaternion _buildRot;

        public int index;

        private void Awake()
        {
            // _cam = Camera.main;
            _outline = GetComponent<Outline>();
            _towerController = BuildController.Instance;
            _outline.enabled = false;
            _buildPos = transform.position;
            _buildRot = transform.rotation;
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
            _towerController.numOfBuildingPoint = index;
            _towerController.OpenBuildPanel(_buildPos + Vector3.up * 10);
            _towerController.SetBuildPoint(_buildPos, _buildRot);
        }
    }
}