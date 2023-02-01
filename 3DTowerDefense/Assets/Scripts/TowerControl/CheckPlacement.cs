using ManagerControl;
using UnityEngine;

namespace TowerControl
{
    public class CheckPlacement : MonoBehaviour
    {
        private BuildingManager _buildingManager;

        private void Awake()
        {
            _buildingManager = GameObject.Find("BuildingManager").GetComponent<BuildingManager>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Tower"))
            {
                _buildingManager.canPlace = false;
            }
            else if (other.CompareTag("Ground"))
            {
                gameObject.SetActive(false);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Tower"))
            {
                _buildingManager.canPlace = true;
            }
        }
    }
}