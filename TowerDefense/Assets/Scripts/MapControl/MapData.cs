using System;
using System.Collections.Generic;
using UnityEngine;

namespace MapControl
{
    public class MapData : MonoBehaviour
    {
        [Flags]
        private enum DirectionFlag
        {
            Straight = 1 << 0,
            Left = 1 << 1,
            Right = 1 << 2
        }

        [SerializeField] private bool isPortalMap;
        [SerializeField] private DirectionFlag wayDirectionFlag;

        public List<Vector3> wayPointList { get; private set; }
        public List<Vector3> placementTile { get; private set; }

        private void Awake()
        {
            wayPointList = new List<Vector3>();
        }

        public void SetWayPoint(int halfMapSize)
        {
            wayPointList.Add(-transform.forward);

            if ((wayDirectionFlag & DirectionFlag.Straight) != 0)
            {
                wayPointList.Add(transform.forward);
            }

            if ((wayDirectionFlag & DirectionFlag.Left) != 0)
            {
                wayPointList.Add(-transform.right);
            }

            if ((wayDirectionFlag & DirectionFlag.Right) != 0)
            {
                wayPointList.Add(transform.right);
            }

            for (var i = 0; i < wayPointList.Count; i++)
            {
                wayPointList[i] *= halfMapSize;
                wayPointList[i] += transform.position;
            }

            PlacementTileInit();
        }

        private void PlacementTileInit()
        {
            var t = transform;
            var tRight = t.right;
            var tForward = t.forward;

            placementTile = new List<Vector3>
            {
                -tRight + tForward,
                tRight + tForward,
                -tRight - tForward,
                tRight - tForward
            };

            var straight = (wayDirectionFlag & DirectionFlag.Straight) != 0;
            var left = (wayDirectionFlag & DirectionFlag.Left) != 0;
            var right = (wayDirectionFlag & DirectionFlag.Right) != 0;

            if (!straight && !isPortalMap) placementTile.Add(tForward);
            if (!left) placementTile.Add(-tRight);
            if (!right) placementTile.Add(tRight);

            for (var i = 0; i < placementTile.Count; i++)
            {
                placementTile[i] *= 4;
                placementTile[i] += t.position + new Vector3(0, 1, 0);
            }
        }
    }
}