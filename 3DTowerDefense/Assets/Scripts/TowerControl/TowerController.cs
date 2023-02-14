using System;
using BuildControl;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TowerControl
{
    public class TowerController : MonoBehaviour
    {
        [SerializeField] private BuildingPoint[] towerPositions;

        private void Awake()
        {
            towerPositions = new BuildingPoint[transform.GetChild(0).transform.childCount];

            for (int i = 0; i < towerPositions.Length; i++)
            {
                var t = transform.GetChild(0).transform.GetChild(i).GetComponent<BuildingPoint>();
                towerPositions[i] = t;
                t.index = i;
            }
        }

        public void DeActiveBuildingPoint(int index)
        {
            towerPositions[index].gameObject.SetActive(false);
        }
    }
}