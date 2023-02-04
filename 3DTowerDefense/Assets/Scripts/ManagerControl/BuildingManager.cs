using DG.Tweening;
using GameControl;
using UnityEngine;
using UnityEngine.UI;

namespace ManagerControl
{
    public class BuildingManager : MonoBehaviour
    {
        private Camera _cam;

        private Ray _camRay;
        private RaycastHit _hit;

        private Vector3 _cursorPos;

        private GameObject _tower;
        private GameObject _selectedObject;

        private bool _isBuilding, _isEditMode, _isGridMove;

        public bool canPlace;

        [SerializeField] private InputManager input;

        [SerializeField] private LayerMask groundLayer, towerLayer;

        [SerializeField] private GameObject buildModePanel;
        [SerializeField] private GameObject editModePanel;

        [SerializeField] private GameObject[] towerName;
        [SerializeField] private Button[] towerButtons;

        [Space(10)] [Header("Camera Tween")] [SerializeField]
        private float duration;

        [SerializeField] private float strength;

        private void Awake()
        {
            _cam = Camera.main;
            input.OnCursorPositionEvent += CursorPosition;
            // input.OnCursorPositionEvent += CheckCursorOutOfScreen;

            input.OnBuildTowerEvent += BuildTower;
            input.OnBuildCancelEvent += CancelBuildMode;

            input.OnSelectTowerEvent += LefClick;
            input.OnSelectCancelEvent += CancelEditMode;
            input.OnSelectCancelEvent += DeSelect;

            towerButtons = new Button[towerName.Length];
            for (var i = 0; i < towerButtons.Length; i++)
            {
                towerButtons[i] = buildModePanel.transform.GetChild(i).GetComponent<Button>();
                var ii = i;
                towerButtons[i].onClick.AddListener(() => ActiveBuildMode(ii));
            }
        }

        private void CursorPosition(Vector2 cursorPos)
        {
            _camRay = _cam.ScreenPointToRay(cursorPos);
            if (Physics.Raycast(_camRay, out _hit, 1000, groundLayer))
            {
                if (_isGridMove)
                {
                    GridCursor();
                }
                else
                {
                    _cursorPos = _hit.point;
                }
            }

            if (_tower != null && _isBuilding) _tower.transform.position = _cursorPos;
        }

        // private void CheckCursorOutOfScreen(Vector2 cursorPos)
        // {
        //     var c = _cam.ScreenToViewportPoint(cursorPos);
        //
        //     if (c.x is < 0 or > 1 || c.y is < 0 or > 1)
        //     {
        //         input.OutOfMouse();     //고쳐야 할수도
        //     }
        // }

        private void GridCursor()
        {
            _cursorPos.x = Mathf.Round(_hit.point.x);
            _cursorPos.y = Mathf.Round(_hit.point.y);
            _cursorPos.z = Mathf.Round(_hit.point.z);
        }

        private void LefClick()
        {
            if (!Physics.Raycast(_camRay, out var t, 1000, towerLayer)) return;
            ActiveEditMode(t);
            SelectedTower(t);
        }

        private void ActiveBuildMode(int index)
        {
            if (_tower == null)
            {
                _tower = StackObjectPool.Get(towerName[index].name, _cursorPos);
            }
            else
            {
                _tower.gameObject.SetActive(false);
                _tower = StackObjectPool.Get(towerName[index].name, _cursorPos);
            }

            if (_isBuilding) return;
            input.ActiveBuildMode();
            _isBuilding = true;
            DeSelect();
        }

        private void CancelBuildMode()
        {
            if (_isBuilding) _isBuilding = false;
            if (_tower != null) _tower.SetActive(false);
        }

        private void BuildTower()
        {
            if (!canPlace || _tower == null) return;
            _isBuilding = false;
            _tower = null;
            _cam.DOShakePosition(duration, strength, randomness: 180);
        }

        private void ActiveEditMode(RaycastHit tower)
        {
            if (_isEditMode) return;
            input.ActiveEditMode();
            _isEditMode = true;
            _tower = tower.collider.gameObject;
        }

        private void CancelEditMode()
        {
            if (_isEditMode) _isEditMode = false;
            _tower = null;
        }

        private void SelectedTower(RaycastHit tower)
        {
            var obj = tower.collider.gameObject;
            if (_selectedObject == obj) return;
            if (_selectedObject != null) DeSelect();
            var outLine = obj.GetComponent<Outline>();
            if (outLine == null) obj.AddComponent<Outline>();
            else outLine.enabled = true;
            _selectedObject = obj;
            if (!editModePanel.activeSelf) editModePanel.SetActive(true);
            editModePanel.transform.position = _cam.WorldToScreenPoint(_cursorPos);
        }

        private void DeSelect()
        {
            if (_selectedObject == null) return;
            _selectedObject.GetComponent<Outline>().enabled = false;
            _selectedObject = null;
            if (editModePanel.activeSelf) editModePanel.SetActive(false);
        }
    }
}