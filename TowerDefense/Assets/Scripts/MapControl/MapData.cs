using System;
using System.Collections.Generic;
using UnityEngine;

namespace MapControl
{
    public class MapData : MonoBehaviour
    {
        [Flags]
        private enum Direction
        {
            Straight = 1 << 0,
            Left = 1 << 1,
            Right = 1 << 2
        }

        [SerializeField] private Direction wayDirection;

        public List<Vector3> wayPointList;

        private void Awake()
        {
            wayPointList = new List<Vector3>();
        }
        
        public void SetWayPoint(int halfMapSize)
        {
            wayPointList.Add(-transform.forward);

            if ((wayDirection & Direction.Straight) != 0)
            {
                wayPointList.Add(transform.forward);
            }

            if ((wayDirection & Direction.Left) != 0)
            {
                wayPointList.Add(-transform.right);
            }

            if ((wayDirection & Direction.Right) != 0)
            {
                wayPointList.Add(transform.right);
            }

            for (int i = 0; i < wayPointList.Count; i++)
            {
                wayPointList[i] *= halfMapSize;
                wayPointList[i] += transform.position;
            }
        }
    }
}