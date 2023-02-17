using System.ComponentModel;
using GameControl;
using ManagerControl;
using TowerControl;
using UnityEngine;
using UnityEngine.Serialization;

namespace BuildControl
{
    public class BuildController : Singleton<BuildController>
    {
        private Vector3 _buildPosition;
        private Quaternion _buildRotation;

        private int _towerIndex;
        private GameObject _selectedTower;

        private int _numOfBuildingPoint;

        public Tower Tower { get; set; }

        [SerializeField] private InputManager input;

        [SerializeField] private BuildingPointController buildingPointController;

        [SerializeField] private GameObject buildPanel;
        [SerializeField] private GameObject buildOkButton;

        [SerializeField] private string[] towersName;

        [SerializeField] private GameObject[] towerButtons;
        [SerializeField] private GameObject[] towerPrefabs;

        private void Start()
        {
            towerButtons = new GameObject[buildPanel.transform.childCount];
            for (var i = 0; i < towerButtons.Length; i++)
            {
                towerButtons[i] = buildPanel.transform.GetChild(i).gameObject;
            }

            input.OnCancelPanelEvent += CloseBuildPanel;
            buildPanel.SetActive(false);
            buildOkButton.SetActive(false);
        }

        public void OpenBuildPanel(int index, Vector3 pos, Quaternion rot)
        {
            if (buildPanel.activeSelf) return;
            _numOfBuildingPoint = index;
            _buildPosition = pos;
            _buildRotation = rot;
            input.isBuild = true;
            buildPanel.transform.position = pos + Vector3.up * 10;
            buildPanel.SetActive(true);
        }

        private void CloseBuildPanel()
        {
            input.isBuild = false;
            buildPanel.SetActive(false);
            buildOkButton.SetActive(false);
            if (!_selectedTower) return;
            _selectedTower.SetActive(false);
        }

        public void BuildTowerButton(int index)
        {
            if (_selectedTower) _selectedTower.SetActive(false);
            _selectedTower = towerPrefabs[index];
            _selectedTower.transform.SetPositionAndRotation(_buildPosition, _buildRotation);
            _selectedTower.SetActive(true);
            _towerIndex = index;
            buildOkButton.transform.position = towerButtons[index].transform.position;
            buildOkButton.SetActive(true);
        }

        public void BuildOkButton()
        {
            StackObjectPool.Get(towersName[_towerIndex], _buildPosition, _buildRotation);
            buildingPointController.DeActiveBuildingPoint(_numOfBuildingPoint);
            CloseBuildPanel();
        }

        public void UpgradeTower()
        {
            Tower.UpgradeTower();
        }
    }
}