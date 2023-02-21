using ManagerControl;
using TowerControl;
using UnityEngine;

namespace BuildControl
{
    [DisallowMultipleComponent]
    public class BuildController : MonoBehaviour
    {
        private Vector2 _cursorPos;
        private Camera _cam;

        private BuildCanvasController _buildCanvasController;
        private EditCanvasController _editCanvasController;

        [SerializeField] private InputManager input;

        [SerializeField] private LayerMask towerLayer;
        [SerializeField] private LayerMask buildingPointLayerMask;


        private void Awake()
        {
            _cam = Camera.main;
            _buildCanvasController = BuildCanvasController.Instance;
            _editCanvasController = EditCanvasController.Instance;
            input.OnCursorPositionEvent += CursorPosition;
            input.OnClickEvent += ClickCheck;
        }

        private void CursorPosition(Vector2 pos)
        {
            _cursorPos = pos;
        }

        private void ClickCheck()
        {
            var r = _cam.ScreenPointToRay(_cursorPos);
// ======================================If BuildingPoint=========================================================

            if (Physics.Raycast(r, out var hit, 1000, buildingPointLayerMask))
            {
                var position = hit.transform.position;
                _buildCanvasController.OpenBuildPanel(hit.transform.GetComponent<BuildingPoint>().index, position,
                    hit.transform.rotation, _cam.WorldToScreenPoint(position));
            }

//============================================If Tower===========================================================

            if (Physics.Raycast(r, out hit, 1000, towerLayer))
            {
                _editCanvasController.selectedTower = hit.transform.GetComponent<Tower>();
                var s = _editCanvasController.selectedTower;
                _editCanvasController.isUnique = s.towerLevel >= 2;
                _editCanvasController.OpenEditPanel(_cam.WorldToScreenPoint(hit.transform.position),
                    s.towerManager.towerLevels[s.towerLevel + 1].towerInfo,
                    s.towerManager.towerLevels[s.towerLevel + 1].towerName);
                print(s.towerLevel);
            }
        }
    }
}