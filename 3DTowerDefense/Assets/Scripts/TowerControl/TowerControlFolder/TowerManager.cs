using BuildControl;
using GameControl;
using ManagerControl;
using UnityEngine;

namespace TowerControl.TowerControlFolder
{
    public class TowerManager : MonoBehaviour
    {
        private Vector3 _buildPosition;
        private Quaternion _buildRotation;

        [SerializeField] private Transform buildingPoint;

        [SerializeField] private BuildingPoint[] buildingPoints;

        private void Awake()
        {
            buildingPoints = new BuildingPoint[buildingPoint.childCount];
            for (var i = 0; i < buildingPoints.Length; i++)
            {
                buildingPoints[i] = buildingPoint.GetChild(i).GetComponent<BuildingPoint>();
                buildingPoints[i].OnOpenTowerSelectPanelEvent += OpenTowerSelectPanel;
            }
        }

        private void OpenTowerSelectPanel(GameObject buildPoint, Transform t, Quaternion r)
        {
            _buildPosition = t.position;
            _buildRotation = r;
            UIManager.Instance.OpenTowerSelectPanel(buildPoint);
        }

        public Tower SpawnTower(string n)
        {
            return StackObjectPool.Get<Tower>(n, _buildPosition, _buildRotation);
        }
    }
}