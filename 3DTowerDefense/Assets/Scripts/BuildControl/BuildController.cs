using GameControl;
using ManagerControl;
using TowerControl;
using UnityEngine;

namespace BuildControl
{
    public class BuildController : Singleton<BuildController>
    {
        private Vector3 _buildPosition;
        private Quaternion _buildRotation;

        public int numOfBuildingPoint;

        public Tower Tower { get; set; }

        [SerializeField] private InputManager input;

        [SerializeField] private BuildingPointController buildingPointController;

        [SerializeField] private GameObject buildPanel;

        [SerializeField] private string[] towers;

        private void Start()
        {
            input.OnCancelPanelEvent += CloseBuildPanel;
        }

        public void OpenBuildPanel(Vector3 pos, Quaternion rot)
        {
            CloseBuildPanel();
            if (buildPanel.activeSelf) return;
            _buildPosition = pos;
            _buildRotation = rot;
            buildPanel.SetActive(true);
            input.isBuild = true;
            buildPanel.transform.position = pos + Vector3.up * 10;
        }

        private void CloseBuildPanel()
        {
            if (!buildPanel.activeSelf) return;
            input.isBuild = false;
            buildPanel.SetActive(false);
            Tower = null;
        }

        public void BuildTowerButton(int index)
        {
            buildingPointController.DeActiveBuildingPoint(numOfBuildingPoint);

            if (Tower == null)
            {
                Tower = StackObjectPool.Get<Tower>(towers[index], _buildPosition, _buildRotation);
            }
            else
            {
                Tower.gameObject.SetActive(false);
                Tower = null;
                Tower = StackObjectPool.Get<Tower>(towers[index], _buildPosition, _buildRotation);
            }

            CloseBuildPanel();
        }

        public void UpgradeTower()
        {
            Tower.UpgradeTower();
        }
    }
}