using UnityEngine;

namespace MapControl
{
    public class MapController : MonoBehaviour
    {
        private Transform[] _wayPoints;

        public Transform TowerBuildPoint => towerBuildPoint;

        [SerializeField] private Transform towerBuildPoint;
        [SerializeField] private Transform wayPointParent;

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