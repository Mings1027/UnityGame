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
                    .OnOpenTowerSelectPanelEvent += OpenTowerSelectPanel;
            }
        }

        private void OpenTowerSelectPanel(GameObject buildPoint, Transform t, Quaternion r)
        {
            _buildPosition = t.position;
            _buildRotation = r;
            UIManager.Instance.OpenTowerSelectPanel(buildPoint);
        }

        public TowerBase BuildTower(string n)
        {
            return StackObjectPool.Get<TowerBase>(n, _buildPosition, _buildRotation);
        }
    }
}