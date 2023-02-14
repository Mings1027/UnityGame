using GameControl;
using ManagerControl;
using TowerControl;
using UnityEngine;

namespace BuildControl
{
    public class BuildController : Singleton<BuildController>
    {
        private Camera _cam;
        private GameObject _tower;
        private Vector3 _buildPos;

        public int numOfBuildingPoint;

        [SerializeField] private GameObject buildModePanel;
        [SerializeField] private TowerController towerController;

        [SerializeField] private InputController input;

        [SerializeField] private string[] towers;

        private void Awake()
        {
            _cam = Camera.main;
        }

        private void Start()
        {
            input.OnCancelBuildEvent += CloseBuildPanel;

            buildModePanel.SetActive(false);
        }

        public void OpenBuildPanel(Vector3 pos)
        {
            CloseBuildPanel();
            input.isBuild = true;
            buildModePanel.SetActive(true);
            buildModePanel.transform.position = pos;
            _buildPos = _cam.ScreenToWorldPoint(pos);
        }

        private void CloseBuildPanel()
        {
            if (!buildModePanel.activeSelf) return;
            input.isBuild = false;
            buildModePanel.SetActive(false);
        }

        public void BuildTower(int index)
        {
            towerController.DeActiveBuildingPoint(numOfBuildingPoint);
            if (_tower == null)
            {
                _tower = StackObjectPool.Get(towers[index], _buildPos);
            }
            else
            {
                _tower = null;
                _tower = StackObjectPool.Get(towers[index], _buildPos);
            }
        }
    }
}