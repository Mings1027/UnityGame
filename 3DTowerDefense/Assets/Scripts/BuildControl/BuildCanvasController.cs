using System;
using GameControl;
using ManagerControl;
using TowerControl;
using UnityEngine;

namespace BuildControl
{
    public class BuildCanvasController : BuildController
    {
        private Vector3 _buildPosition;
        private Quaternion _buildRotation;

        private int _towerIndex;
        private int _numOfBuildingPoint;
        private GameObject _tempSelectedTower;

        [SerializeField] private InputManager input;
        [SerializeField] private BuildingPointController buildingPointController;

        [SerializeField] private GameObject buildPanel;
        [SerializeField] private GameObject buildOkButton;
        [SerializeField] private GameObject towerPrefab;

        [SerializeField] private GameObject[] towerButtons;
        [SerializeField] private GameObject[] towerPrefabs;
        [SerializeField] private string[] towersName;

        private void Awake()
        {
            input.OnCancelPanelEvent += CloseBuildPanel;
            towerButtons = new GameObject[buildPanel.transform.childCount];
            for (var i = 0; i < towerButtons.Length; i++)
            {
                towerButtons[i] = buildPanel.transform.GetChild(i).gameObject;
            }

            towerPrefabs = new GameObject[towerPrefab.transform.childCount];
            towersName = new string[towerPrefabs.Length];
            for (var i = 0; i < towerPrefabs.Length; i++)
            {
                towerPrefabs[i] = towerPrefab.transform.GetChild(i).gameObject;
                towersName[i] = towerPrefabs[i].name;
            }
        }

        private void Start()
        {
            buildPanel.SetActive(false);
            buildOkButton.SetActive(false);
        }

        public void OpenBuildPanel(int index, Vector3 pos, Quaternion rot, Vector3 cursorPos)
        {
            if (buildPanel.activeSelf) return;
            _numOfBuildingPoint = index;
            _buildPosition = pos;
            _buildRotation = rot;
            input.isBuild = true;
            buildPanel.transform.position = cursorPos;
            buildPanel.SetActive(true);
        }

        private void CloseBuildPanel()
        {
            input.isBuild = false;
            buildPanel.SetActive(false);
            buildOkButton.SetActive(false);
            if (!_tempSelectedTower) return;
            _tempSelectedTower.SetActive(false);
        }
        
        public void TowerSelectButton(int index)
        {
            if (_tempSelectedTower) _tempSelectedTower.SetActive(false);
            _tempSelectedTower = towerPrefabs[index];
            _tempSelectedTower.transform.SetPositionAndRotation(_buildPosition, _buildRotation);
            _tempSelectedTower.SetActive(true);
            _towerIndex = index;
            buildOkButton.transform.position = towerButtons[index].transform.position;
            buildOkButton.SetActive(true);
        }

        //BuildPanel -> BuildOkButton
        public void TowerBuildButton()
        {
            SelectedTower = StackObjectPool.Get<Tower>(towersName[_towerIndex], _buildPosition, _buildRotation);
            buildingPointController.DeActiveBuildingPoint(_numOfBuildingPoint);
            CloseBuildPanel();
        }
    }
}