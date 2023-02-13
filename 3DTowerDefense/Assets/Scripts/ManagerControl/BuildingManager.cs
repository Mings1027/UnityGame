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

        [SerializeField] private InputController input;

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

            input.OnClickTowerEvent += SelectTower;

            input.OnBuildTowerEvent += BuildTower;
            input.OnBuildCancelEvent += CancelBuildMode;

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

        private void GridCursor()
        {
            _cursorPos.x = Mathf.Round(_hit.point.x);
            _cursorPos.y = Mathf.Round(_hit.point.y);
            _cursorPos.z = Mathf.Round(_hit.point.z);
        }

        private void SelectTower()
        {
            if (!Physics.Raycast(_camRay, out var t, 1000, towerLayer) || _tower != null) return;
            ActiveEditMode(t);
            OutLineTower(t);
        }

        private void ActiveBuildMode(int index)
        {
            print("clickkkkkk");
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
            _isBuilding = true;
            input.OnBuildMode();
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
            print("build!");
            _isBuilding = false;
            _tower = null;
            _cam.DOShakePosition(duration, strength, randomness: 180);
        }

        private void ActiveEditMode(RaycastHit tower)
        {
            if (_isEditMode) return;
            _isEditMode = true;
            _tower = tower.collider.gameObject;
        }

        private void CancelEditMode()
        {
            if (_isEditMode) _isEditMode = false;
            _tower = null;
        }

        private void OutLineTower(RaycastHit tower)
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