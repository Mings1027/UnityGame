using GameControl;
using ManagerControl;
using TowerControl;
using UnityEngine;

namespace BuildControl
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
                    .onOpenTowerSelectPanelEvent += OpenTowerSelectPanel;
            }
        }

        private void OpenTowerSelectPanel(Transform buildPoint)
        {
            _buildPosition = buildPoint.position;
            _buildRotation = buildPoint.rotation;
            UIManager.Instance.OpenTowerSelectPanel(buildPoint);
        }

        public Tower BuildTower(string n)
        {
            return StackObjectPool.Get<Tower>(n, _buildPosition, _buildRotation);
        }
    }
}