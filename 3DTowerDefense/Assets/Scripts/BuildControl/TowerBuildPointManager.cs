using BuildControl;
using GameControl;
using ManagerControl;
using UnityEngine;

namespace TowerControl.TowerControlFolder
{
    public class TowerBuildPointManager : MonoBehaviour
    {
        private Vector3 _buildPosition;
        private Quaternion _buildRotation;

        private void Awake()
        {
            for (var i = 0; i < transform.childCount; i++)
            {
                var child = transform.GetChild(i);
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