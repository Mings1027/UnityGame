using BuildControl;
using UnityEngine;

namespace TowerControl
{
    public class BuildingPointController : MonoBehaviour
    {
        [SerializeField] private BuildingPoint[] buildingPositions;

        private void Awake()
        {
            var thisChild = transform.GetChild(0);
            buildingPositions = new BuildingPoint[thisChild.transform.childCount];

            for (var i = 0; i < buildingPositions.Length; i++)
            {
                var t = thisChild.transform.GetChild(i).GetComponent<BuildingPoint>();
                buildingPositions[i] = t;
                t.index = i;
            }
        }

        public void ActiveBuildingPoint(int index)
        {
            print(index);
            buildingPositions[index].gameObject.SetActive(true);
        }
        //When Built Tower
        public void DeActiveBuildingPoint(int index)
        {
            buildingPositions[index].gameObject.SetActive(false);
        }
    }
}