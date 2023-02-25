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
        
        private void Awake()
        {
            for (var i = 0; i < buildingPoint.childCount; i++)
            {
                var child = buildingPoint.GetChild(i);
                StackObjectPool.Get<BuildingPoint>("BuildingPoint", child.position, child.rotation)
                    .OnOpenTowerSelectPanelEvent += OpenTowerSelectPanel;
            }
        }

        private void OpenTowerSelectPanel(GameObject buildPoint, Transform t, Quaternion r)
        {
            _buildPosition = t.position;
            _buildRotation = r;
            UIManager.Instance.OpenTowerSelectPanel(buildPoint);
        }

        public Tower BuildTower(string n)
        {
            return StackObjectPool.Get<Tower>(n, _buildPosition, _buildRotation);
        }
    }
}