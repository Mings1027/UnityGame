using GameControl;
using ManagerControl;
using TowerControl;
using UnityEngine;
using UnityEngine.Serialization;

namespace BuildControl
{
    public class BuildCanvasController : Singleton<BuildCanvasController>
    {
        private Vector3 _buildPosition;
        private Quaternion _buildRotation;

        private int _towerIndex;
        private int _numOfBuildingPoint;
        private Tower _tempSelectedTower;

        [SerializeField] private InputManager input;
        [SerializeField] private BuildingPointController buildingPointController;

        [SerializeField] private GameObject buildPanel;
        [SerializeField] private GameObject buildOkButton;

        [SerializeField] private GameObject[] towerButtons;
        [SerializeField] private string[] towersName;

        private void Awake()
        {
            input.OnCancelPanelEvent += CloseBuildPanel;

            towerButtons = new GameObject[buildPanel.transform.childCount];
            towersName = new string[towerButtons.Length];
            for (var i = 0; i < towerButtons.Length; i++)
            {
                towerButtons[i] = buildPanel.transform.GetChild(i).gameObject;
                towersName[i] = towerButtons[i].name.Replace("Button", "");
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
            _tempSelectedTower.gameObject.SetActive(false);
        }

        public void TowerSelectButton(int index)
        {
            if (_tempSelectedTower) _tempSelectedTower.gameObject.SetActive(false);
            _tempSelectedTower = StackObjectPool.Get<Tower>(towersName[index], _buildPosition, _buildRotation);
            buildOkButton.transform.position = towerButtons[index].transform.position;
            buildOkButton.SetActive(true);
            _towerIndex = index;
        }

        private void ShowTowerInfo()
        {
            
        }

        //BuildPanel -> BuildOkButton
        public void TowerBuildButton()
        {
            StackObjectPool.Get<Tower>(towersName[_towerIndex], _buildPosition, _buildRotation).Init();
            buildingPointController.DeActiveBuildingPoint(_numOfBuildingPoint);
            CloseBuildPanel();
        }
    }
}