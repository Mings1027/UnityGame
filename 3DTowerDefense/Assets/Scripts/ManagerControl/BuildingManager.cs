using System;
using DG.Tweening;
using GameControl;
using UnityEngine;
using UnityEngine.InputSystem;
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

        private bool _isBuilding, _isEditMode;

        private Button[] _towerButtons;

        public bool canPlace;

        [SerializeField] private InputManager input;
        [SerializeField] private GameManager gameManager;

        [SerializeField] private LayerMask layer;
        [SerializeField] private LayerMask towerLayer;

        [SerializeField] private GameObject buildModePanel;
        [SerializeField] private GameObject editModePanel;

        [SerializeField] private GameObject[] towerName;

        [SerializeField] private bool gridMove;

        [Space(10)] [Header("Camera Tween")] [SerializeField]
        private float duration;

        [SerializeField] private float strength;

        private void Awake()
        {
            _cam = Camera.main;
            input.OnCursorPositionEvent += CursorPosition;
            input.OnCursorPositionEvent += CheckMousePosition;

            input.OnLeftClickEvent += LeftClick;

            input.OnRightClickEvent += CancelEditMode;
            input.OnRightClickEvent += DeSelect;

            _towerButtons = new Button[towerName.Length];
            for (var i = 0; i < _towerButtons.Length; i++)
            {
                _towerButtons[i] = buildModePanel.transform.GetChild(i).GetComponent<Button>();
                var i1 = i;
                _towerButtons[i].onClick.AddListener(() => SpawnTower(i1));
            }
        }

        private void CursorPosition(Vector2 cursorPos)
        {
            _camRay = _cam.ScreenPointToRay(cursorPos);
            if (Physics.Raycast(_camRay, out _hit, 1000, layer))
            {
                if (gridMove)
                {
                    GridCursor();
                }
                else
                {
                    _cursorPos = _hit.point;
                }
            }

            if (_tower is not null && _isBuilding) _tower.transform.position = _cursorPos;
        }

        private void CheckMousePosition(Vector2 cursorPos)
        {
            var pos = _cam.ScreenToViewportPoint(cursorPos);

            if (_isBuilding && (pos.x < 0 || pos.y < 0 || pos.x > 1 || pos.y > 1))
            {
                gameManager.Pause();
            }
        }

        private void GridCursor()
        {
            _cursorPos.x = Mathf.Round(_hit.point.x);
            _cursorPos.y = Mathf.Round(_hit.point.y);
            _cursorPos.z = Mathf.Round(_hit.point.z);
        }

        private void LeftClick()
        {
            if (_isBuilding)
            {
                PlaceTower();
            }
            else
            {
                if (!Physics.Raycast(_camRay, out var t, 1000, towerLayer)) return;
                ActiveEditMode(t);
                SelectTower(t);
            }
        }

        // //EditModePanel ON
        private void ActiveEditMode(RaycastHit t)
        {
            _tower = t.collider.gameObject;
            if (!_isEditMode) _isEditMode = true;
            if (!editModePanel.activeSelf) editModePanel.SetActive(true);
            editModePanel.transform.position = _cam.WorldToScreenPoint(_cursorPos);
        }

        // //EditModePanel OFF
        private void CancelEditMode()
        {
            if (_isEditMode) _isEditMode = false;
            if (editModePanel.activeSelf) editModePanel.SetActive(false);
            _tower = null;
        }

        public void SpawnTower(int index)
        {
            if (_isEditMode) return;
            _isBuilding = true;
            if (_tower != null)
            {
                _tower.gameObject.SetActive(false);
                _tower = StackObjectPool.Get(towerName[index].name, _cursorPos);
            }
            else
            {
                _tower = StackObjectPool.Get(towerName[index].name, _cursorPos);
            }
        }

        private void PlaceTower()
        {
            if (canPlace && _tower != null)
            {
                _isBuilding = false;
                _tower = null;
                _cam.DOShakePosition(duration, strength, randomness: 180);
            }
        }

        private void SelectTower(RaycastHit raycastHit)
        {
            var obj = raycastHit.collider.gameObject;
            if (obj == _selectedObject) return;
            if (_selectedObject is not null) DeSelect();
            var outLine = obj.GetComponent<Outline>();
            if (outLine is null) obj.AddComponent<Outline>();
            else outLine.enabled = true;
            _selectedObject = obj;
        }

        private void DeSelect()
        {
            if (_selectedObject is null) return;
            _selectedObject.GetComponent<Outline>().enabled = false;
            _selectedObject = null;
        }
    }
}