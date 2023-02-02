using System.Collections.Generic;
using DG.Tweening;
using GameControl;
using TowerControl;
using UnityEngine;
using UnityEngine.UI;

namespace ManagerControl
{
    public class BuildingManager : MonoBehaviour
    {
        private Camera _cam;

        private readonly List<GameObject> _towerList = new();
        private readonly float YHeight = 0.51f;

        private bool _isBuildMode, _isEditMode;

        private Ray _camRay;
        private Vector3 _cursorPos;
        private RaycastHit _hit;

        private Vector3 _buildPos;
        public bool canPlace;

        private Tower _tower;
        
        [SerializeField] private InputManager input;
        [SerializeField] private LayerMask cursorMoveLayer;
        [SerializeField] private GameObject[] towerName;
        [SerializeField] private Button[] towerButtons;

        [SerializeField] private Transform cursorObj;

        [SerializeField] private GameObject buildModePanel;
        [SerializeField] private GameObject editModePanel;

        private void Awake()
        {
            _cam = Camera.main;
            input.OnCursorPositionEvent += CursorPosition;
            input.OnLeftClickEvent += LeftClick;
            input.OnBuildModeEvent += ActiveBuildMode;
            input.OnCancelModeEvent += CancelBuildMode;
            input.OnCancelModeEvent += CancelEditMode;

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
            if (Physics.Raycast(_camRay, out _hit, Mathf.Infinity, cursorMoveLayer))
            {
                _cursorPos.x = Mathf.Round(_hit.point.x);
                _cursorPos.y = Mathf.Round(_hit.point.y) + YHeight;
                _cursorPos.z = Mathf.Round(_hit.point.z);

                cursorObj.position = _cursorPos;
            }
        }

        private void LeftClick()
        {
            if (UiManager.OnPointer is false)
            {
                if (_hit.collider.CompareTag("Tower"))
                {
                    ActiveEditMode();
                }
                else
                {
                    CancelBuildMode();
                    CancelEditMode();
                }
            }
        }

        //buildModePanel ON
        private void ActiveBuildMode()
        {
            // if (_isEditMode is false && _tower is { built: false }) // !isEditMode && _tower != null && !_tower.built
            // {
            //     _tower.gameObject.SetActive(false);
            // }

            if (!_isBuildMode) _isBuildMode = true;
            if (_isEditMode) _isEditMode = false;
            _tower = null;
            if (!buildModePanel.activeSelf) buildModePanel.SetActive(true);
            if (editModePanel.activeSelf) editModePanel.SetActive(false);
            buildModePanel.transform.position = _cam.WorldToScreenPoint(_cursorPos);
            _buildPos = _cursorPos;
        }

        //EditModePanel ON
        private void ActiveEditMode()
        {
            _tower = _hit.collider.GetComponent<Tower>();

            if (!_isEditMode) _isEditMode = true;
            if (_isBuildMode) _isBuildMode = false;
            if (!editModePanel.activeSelf) editModePanel.SetActive(true);
            if (buildModePanel.activeSelf) buildModePanel.SetActive(false);
            editModePanel.transform.position = _cam.WorldToScreenPoint(_cursorPos);
            print(_cursorPos);
        }

        //BuildModePanel OFF BuildModePanel => Cancel Button
        private void CancelBuildMode()
        {
            if (_isBuildMode) _isBuildMode = false;
            if (buildModePanel.activeSelf) buildModePanel.SetActive(false);
            if (editModePanel.activeSelf) editModePanel.SetActive(false);
            if (_tower is { built: false }) //_tower is not null && !_tower.built
            {
                _tower.gameObject.SetActive(false);
            }
        }

        //EditModePanel OFF
        private void CancelEditMode()
        {
            if (_isEditMode) _isEditMode = false;
            if (editModePanel.activeSelf) editModePanel.SetActive(false);
            if (buildModePanel.activeSelf) buildModePanel.SetActive(false);
        }

        public void SellTower()
        {
            print("Sell Tower!!");
            _tower.gameObject.SetActive(false);
            _tower = null;
            CancelEditMode();
        }

        public void UpgradeTower()
        {
            print("Upgrade Tower!!");
        }

        // public void ShowTower(int index) // Short Press
        // {
        //     if (_tower is null)
        //     {
        //         _tower = StackObjectPool.Get<Tower>(towerName[index].name, _buildPos);
        //     }
        //     else
        //     {
        //         _tower.gameObject.SetActive(false);
        //         _tower = StackObjectPool.Get<Tower>(towerName[index].name, _buildPos);
        //     }
        // }

        public float duration;
        public float strength;

        public void SpawnTower(int index) // Long Press
        {
            // if (_tower is null || _towerNum != index) return;
            _tower = StackObjectPool.Get<Tower>(towerName[index].name, _buildPos);
            _tower.built = true;
            _towerList.Add(_tower.gameObject);
            _tower = null;
            print("Built!");
            _cam.DOShakePosition(duration, strength, randomness: 180);
            CancelBuildMode();
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