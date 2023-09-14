using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

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

        public List<Vector3> diagonalDir { get; private set; }
        public List<Vector3> placementTile { get; private set; }

        [SerializeField] private bool isPortalMap;
        [SerializeField] private Direction wayDirection;

        public List<Vector3> wayPointList { get; private set; }

        private void Awake()
        {
            diagonalDir = new List<Vector3>
            {
                new(1, 0, 1),
                new(-1, 0, 1),
                new(-1, 0, -1),
                new(1, 0, -1)
            };
            wayPointList = new List<Vector3>();
        }
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            for (int i = 0; i < placementTile.Count; i++)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(placementTile[i], 0.5f);
            }
        }
#endif
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

            var straight = (wayDirection & Direction.Straight) != 0;
            var left = (wayDirection & Direction.Left) != 0;
            var right = (wayDirection & Direction.Right) != 0;

            if (!straight)
            {
                if (!isPortalMap)
                    placementTile.Add(tForward);
            }

            if (!left)
            {
                placementTile.Add(-tRight);
            }

            if (!right)
            {
                placementTile.Add(tRight);
            }

            for (var i = 0; i < placementTile.Count; i++)
            {
                placementTile[i] *= 4;
                placementTile[i] += t.position + new Vector3(0, 1, 0);
            }
        }
    }
}