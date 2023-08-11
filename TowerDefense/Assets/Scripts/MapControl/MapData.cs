using UnityEngine;

namespace MapControl
{
    public class MapData : MonoBehaviour
    {
        public Transform[] WayPoints { get; private set; }

        private void Awake()
        {
            var wayPointParent = transform.Find("WayPoints");
            WayPoints = new Transform[wayPointParent.childCount];
            for (int i = 0; i < WayPoints.Length; i++)
            {
                WayPoints[i] = wayPointParent.GetChild(i);
            }
        }
    }
}