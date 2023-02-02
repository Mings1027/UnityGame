using System;
using System.Collections.Generic;
using DG.Tweening;
using GameControl;
using TowerControl;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace ManagerControl
{
    public class BuildingController : MonoBehaviour
    {
        public static BuildingController inst;
        private Camera _cam;

        private Ray _camRay;
        private RaycastHit _hit;

        private Vector3 _cursorPos;

        private Tower _tower;
        private GameObject _selectedObject;

        private bool _isBuildMode, _isEditMode;
        public bool canPlace;

        [SerializeField] private InputManager input;
        [SerializeField] private LayerMask layer;
        [SerializeField] private GameObject[] towerName;
        [SerializeField] private Button[] towerButtons;

        [SerializeField] private GameObject buildModePanel;
        [SerializeField] private GameObject editModePanel;

        [SerializeField] private bool gridMove;

        private void Awake()
        {
            if (inst is null)
            {
                inst = this;
                DontDestroyOnLoad(this);
            }
            else
            {
                if (inst != this)
                    Destroy(this);
            }

            _cam = Camera.main;
            input.OnCursorPositionEvent += CursorPosition;
            input.OnLeftClickEvent += LeftClickCheck;
            input.OnBuildModeEvent += ToggleBuildMode;
            input.OnCancelModeEvent += CancelModes;

            towerButtons = new Button[towerName.Length];
            for (var i = 0; i < towerButtons.Length; i++)
            {
                towerButtons[i] = buildModePanel.transform.GetChild(i).GetComponent<Button>();
                var i1 = i;
                towerButtons[i].onClick.AddListener(() => SpawnTower(i1));
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

            if (_tower is not null) _tower.transform.position = _cursorPos;
        }

        private void GridCursor()
        {
            _cursorPos.x = Mathf.Round(_hit.point.x);
            _cursorPos.y = Mathf.Round(_hit.point.y);
            _cursorPos.z = Mathf.Round(_hit.point.z);
        }

        private void LeftClickCheck()
        {
            if (_isBuildMode && _tower is not null && canPlace)
            {
                PlaceTower();
            }
            else if (_tower is null)
            {
                if (Physics.Raycast(_camRay, out _, maxDistance: 1000))
                {
                    CancelEditMode();
                    DeSelect();
                }
            }
        }

        private void ToggleBuildMode()
        {
            if (_isEditMode) return;
            if (!_isBuildMode) ActiveBuildMode();
            else CancelBuildMode();
        }

        private void ActiveBuildMode()
        {
            DeSelect();
            if (!_isBuildMode) _isBuildMode = true;
            if (_isEditMode) _isEditMode = false;
            _tower = null;
            if (!buildModePanel.activeSelf) buildModePanel.SetActive(true);
            if (editModePanel.activeSelf) editModePanel.SetActive(false);
        }

        private void CancelBuildMode()
        {
            if (_isBuildMode) _isBuildMode = false;
            if (buildModePanel.activeSelf) buildModePanel.SetActive(false);
            if (_tower is { built: false }) //_tower is not null && !_tower.built
            {
                _tower.gameObject.SetActive(false);
            }
        }

        private void CancelModes()
        {
            if (_isBuildMode) CancelBuildMode();
            else if (_isEditMode) CancelEditMode();
            else input.isDoSomething = false;
        }

        // //EditModePanel ON
        public void ActiveEditMode()
        {
            _tower = _hit.collider.GetComponent<Tower>();

            if (!_isEditMode) _isEditMode = true;
            if (_isBuildMode) _isBuildMode = false;
            if (!editModePanel.activeSelf) editModePanel.SetActive(true);
            if (buildModePanel.activeSelf) buildModePanel.SetActive(false);
            editModePanel.transform.position = _cam.WorldToScreenPoint(_cursorPos);
        }
        
        // //EditModePanel OFF
        private void CancelEditMode()
        {
            if (_isEditMode) _isEditMode = false;
            if (editModePanel.activeSelf) editModePanel.SetActive(false);
        }


        public void SpawnTower(int index)
        {
            if (_tower is not null)
            {
                print("tower is not null");
                _tower.gameObject.SetActive(false);
                _tower = StackObjectPool.Get<Tower>(towerName[index].name, _cursorPos);
            }
            else
            {
                print("tower is null");
                _tower = StackObjectPool.Get<Tower>(towerName[index].name, _cursorPos);
            }
        }

        [Header("Camera Tween")] [SerializeField]
        private float duration, strength;

        private void PlaceTower()
        {
            _tower.GetComponent<CheckPlacement>().SetPlaceTower();
            _tower = null;
            _cam.DOShakePosition(duration, strength, randomness: 180);
        }

        public void SelectTower(GameObject obj)
        {
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