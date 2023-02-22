using UnityEngine;

namespace BuildControl
{
    public class BuildingPointController : MonoBehaviour
    {
        [SerializeField] private BuildingPoint[] buildingPositions;

        private void Awake()
        {
            buildingPositions = new BuildingPoint[transform.childCount];

            for (var i = 0; i < buildingPositions.Length; i++)
            {
                var t = transform.GetChild(i).GetComponent<BuildingPoint>();
                buildingPositions[i] = t;
                t.index = i;
            }

            for (var i = 0; i < buildingPositions.Length; i++)
            {
                buildingPositions[i] = transform.GetChild(i).GetComponent<BuildingPoint>();
                buildingPositions[i].OnOpenTowerSelectPanelEvent += OpenTowerSelectPanel;
            }
        }

        private void OpenTowerSelectPanel(BuildingPoint bp,Transform t,Quaternion r)
        {
            
        }


        public void ActiveBuildingPoint(int index)
        {
            buildingPositions[index].gameObject.SetActive(true);
        }
        //When Built Tower
        public void DeActiveBuildingPoint(int index)
        {
            buildingPositions[index].gameObject.SetActive(false);
        }
    }
}