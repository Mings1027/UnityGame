using System;
using BuildControl;
using DataControl;
using GameControl;
using ManagerControl;
using UIControl;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MapControl
{
    public class MapController : MonoBehaviour, IPointerDownHandler,IPointerUpHandler
    {
        [SerializeField] private Transform towerBuildPoint;

        public event Action onCloseUIEvent;

        private void Start()
        {
            CreateBuildPoints();
        }

        private void CreateBuildPoints()
        {
            var gameManager = GameManager.Instance;
            var gamePlayUIController =
                gameManager.UIPrefab.transform.Find("GamePlay UI").GetComponent<GamePlayUIController>();

            for (var i = 0; i < towerBuildPoint.childCount; i++)
            {
                var child = towerBuildPoint.GetChild(i);
                var b = StackObjectPool.Get<BuildingPoint>(PoolObjectName.BuildingPoint, child);
                // b.onOpenTowerSelectPanelEvent += gamePlayUIController.OpenTowerSelectPanel;
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            onCloseUIEvent?.Invoke();
        }
    }
}