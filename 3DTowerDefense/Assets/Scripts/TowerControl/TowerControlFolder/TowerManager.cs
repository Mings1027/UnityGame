using BuildControl;
using GameControl;
using ManagerControl;
using UnityEngine;

namespace TowerControl.TowerControlFolder
{
    public class TowerManager : MonoBehaviour
    {
        private UIManager _uiManager;

        private Vector3 _buildPosition;
        private Quaternion _buildRotation;

        [SerializeField] private Transform buildingPoint;

        [SerializeField] private BuildingPoint[] buildingPoints;

        private void Awake()
        {
            _uiManager = UIManager.Instance;
            buildingPoints = new BuildingPoint[buildingPoint.childCount];
            for (var i = 0; i < buildingPoints.Length; i++)
            {
                buildingPoints[i] = buildingPoint.GetChild(i).GetComponent<BuildingPoint>();
                buildingPoints[i].OnOpenTowerSelectPanelEvent += OpenTowerSelectPanel;
            }
        }

        private void OpenTowerSelectPanel(GameObject bp, Transform t, Quaternion r)
        {
            _buildPosition = t.position;
            _buildRotation = r;
            _uiManager.OpenTowerSelectPanel(bp);
        }

        public Tower ShowTempTower(string n)
        {
            return StackObjectPool.Get<Tower>(n, _buildPosition, _buildRotation);
        }

        public Tower BuildTower(string n)
        {
            return StackObjectPool.Get<Tower>(n, _buildPosition, _buildRotation);
        }
    }
}