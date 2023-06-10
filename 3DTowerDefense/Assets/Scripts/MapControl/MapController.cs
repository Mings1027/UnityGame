using UnityEngine;

namespace MapControl
{
    public class MapController : MonoBehaviour
    {
        public Transform TowerBuildPoint => towerBuildPoint;

        [SerializeField] private Transform towerBuildPoint;
        [SerializeField] private Transform wayPointParent;
        private Transform[] _wayPoints;

        private void Start()
        {
            _wayPoints = new Transform[wayPointParent.childCount];
            for (var i = 0; i < _wayPoints.Length; i++)
            {
                _wayPoints[i] = wayPointParent.GetChild(i);
            }
        }
    }
}