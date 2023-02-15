using GameControl;
using ManagerControl;
using TowerControl;
using UnityEngine;

namespace BuildControl
{
    public class BuildController : Singleton<BuildController>
    {
        private GameObject _tower;
        private Vector3 _buildPosition;
        private Quaternion _buildRotation;

        public int numOfBuildingPoint;

        [SerializeField] private InputManager input;

        [SerializeField] private BuildingPointController buildingPointController;

        [SerializeField] private GameObject buildPanel;

        [SerializeField] private string[] towers;

        private void Start()
        {
            input.OnCancelPanelEvent += CloseBuildPanel;
        }

        public void OpenBuildPanel(Vector3 pos)
        {
            CloseBuildPanel();
            if (buildPanel.activeSelf) return;
            buildPanel.SetActive(true);
            input.isBuild = true;
            buildPanel.transform.position = pos;
        }

        public void SetBuildPoint(Vector3 buildPos, Quaternion buildRot)
        {
            _buildPosition = buildPos;
            _buildRotation = buildRot;
        }

        private void CloseBuildPanel()
        {
            if (!buildPanel.activeSelf) return;
            input.isBuild = false;
            buildPanel.SetActive(false);
        }

        public void BuildTower(int index)
        {
            CloseBuildPanel();
            buildingPointController.DeActiveBuildingPoint(numOfBuildingPoint);
            if (_tower == null)
            {
                _tower = StackObjectPool.Get(towers[index], _buildPosition, _buildRotation);
            }
            else
            {
                _tower = null;
                _tower = StackObjectPool.Get(towers[index], _buildPosition, _buildRotation);
            }
        }
    }
}