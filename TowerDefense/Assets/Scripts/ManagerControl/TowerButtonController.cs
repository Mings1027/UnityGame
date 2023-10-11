using CustomEnumControl;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ManagerControl
{
    public class TowerButtonController : MonoBehaviour, IPointerDownHandler
    {
        // private TowerManager _towerManager;
        [SerializeField] private TowerType towerType;
        [SerializeField] private bool isUnitTower;

        // private void Awake()
        // {
        //     _towerManager = GetComponentInParent<TowerManager>();
        // }

        public void OnPointerDown(PointerEventData eventData)
        {
            InputManager.Instance.enabled = true;
            InputManager.Instance.StartPlacement(in towerType, isUnitTower);
        }
    }
}