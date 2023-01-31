using System;
using System.Collections.Generic;
using GameControl;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace ManagerControl
{
    public class BuildingManager : MonoBehaviour
    {
        private Camera _cam;

        private readonly List<GameObject> _towerList = new();
        private GameObject _pendingObject;
        private bool _isBuilding;

        private Ray _camRay;
        private Vector3 _cursorPos;
        private RaycastHit _hit;

        private Vector3 _buildPos;
        public bool canPlace;

        [SerializeField] private InputManager input;
        [SerializeField] private LayerMask cursorMoveLayer;
        [SerializeField] private string[] towerName;

        [SerializeField] private Transform cursorObj;

        [SerializeField] private GameObject buildModePanel;
        [SerializeField] private Button[] towerBuildButtons;

        [SerializeField] private GameObject editModePanel;

        private void Awake()
        {
            input.OnCursorPositionEvent += CursorPosition;
            input.OnActiveBuildModeEvent += InvokeByTag;

            _cam = Camera.main;

            towerBuildButtons = new Button[buildModePanel.transform.childCount];
            for (var i = 0; i < towerBuildButtons.Length; i++)
            {
                towerBuildButtons[i] = buildModePanel.transform.GetChild(i).GetComponent<Button>();
                var i1 = i;
                towerBuildButtons[i].onClick.AddListener(() => PressTowerButton(i1));
            }
        }

        private void CursorPosition(Vector2 cursorPos)
        {
            _camRay = _cam.ScreenPointToRay(cursorPos);
            if (Physics.Raycast(_camRay, out _hit, Mathf.Infinity, cursorMoveLayer))
            {
                _cursorPos.x = Mathf.Round(_hit.point.x);
                _cursorPos.y = Mathf.Round(_hit.point.y) + .51f;
                _cursorPos.z = Mathf.Round(_hit.point.z);

                cursorObj.position = _cursorPos;
            }
        }

        private void InvokeByTag()
        {
            if (UiManager.OnPointer) return;

            _pendingObject = null;

            if (_hit.collider.CompareTag("BuildGround"))
            {
                ActiveBuildMode();
            }
            else if (_hit.collider.CompareTag("Ground"))
            {
                CancelBuildMode();
                CancelEditMode();
            }
            else if (_hit.collider.CompareTag("Tower"))
            {
                _pendingObject = _hit.collider.gameObject;
                ActiveEditMode();
            }
        }

        //buildModePanel ON
        private void ActiveBuildMode()
        {
            if (!_isBuilding) _isBuilding = true;
            if (!buildModePanel.activeSelf) buildModePanel.SetActive(true);
            if (editModePanel.activeSelf) editModePanel.SetActive(false);
            buildModePanel.transform.position = _cam.WorldToScreenPoint(_cursorPos);
            _buildPos = _cursorPos;
        }

        //buildModePanel OFF BuildModePanel => Cancel Button
        public void CancelBuildMode()
        {
            if (_isBuilding) _isBuilding = false;
            if (buildModePanel.activeSelf) buildModePanel.SetActive(false);
            if (editModePanel.activeSelf) editModePanel.SetActive(false);
            if (_pendingObject != null) _pendingObject.SetActive(false);
        }

        //EditModePanel ON
        private void ActiveEditMode()
        {
            if (!editModePanel.activeSelf) editModePanel.SetActive(true);
            if (buildModePanel.activeSelf) buildModePanel.SetActive(false);
            editModePanel.transform.position = _cam.WorldToScreenPoint(_cursorPos);
        }

        //EditModePanel OFF
        private void CancelEditMode()
        {
            if (editModePanel.activeSelf) editModePanel.SetActive(false);
            if (buildModePanel.activeSelf) buildModePanel.SetActive(false);
        }

        public void SellTower()
        {
            print("Sell Tower!!");
            _pendingObject.SetActive(false);
        }

        public void UpgradeTower()
        {
            print("Upgrade Tower!!");
        }

        private void PressTowerButton(int index)
        {
            if (_pendingObject != null)
            {
                _pendingObject.SetActive(false);
                _pendingObject = null;
            }

            _pendingObject = StackObjectPool.Get(towerName[index], _buildPos);

            towerBuildButtons[index].onClick.RemoveAllListeners();
            towerBuildButtons[index].onClick.AddListener(() => SpawnTower(index));
        }

        private void SpawnTower(int index)
        {
            _towerList.Add(_pendingObject);

            towerBuildButtons[index].onClick.RemoveAllListeners();
            towerBuildButtons[index].onClick.AddListener(() => PressTowerButton(index));
        }

        //OnClick Event GenerateNewMap Button Object
        public void TowerReset()
        {
            foreach (var t in _towerList)
            {
                t.SetActive(false);
            }
        }
    }
}