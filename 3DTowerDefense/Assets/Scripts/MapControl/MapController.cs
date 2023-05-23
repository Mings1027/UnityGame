using System;
using BuildControl;
using DG.Tweening;
using GameControl;
using ManagerControl;
using UIControl;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MapControl
{
    public class MapController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public event Action onCloseUIEvent;

        [SerializeField] private Transform towerBuildPoint;

        private void Start()
        {
            CreateBuildPoints();
        }

        private void CreateBuildPoints()
        {
            var gamePlayUIController = GameManager.Instance.GamePlayPrefab.transform.Find("GamePlay UI")
                .GetComponent<GamePlayUIController>();

            for (int i = 0; i < towerBuildPoint.childCount; i++)
            {
                var child = towerBuildPoint.GetChild(i);
                StackObjectPool.Get<BuildingPoint>("BuildingPoint", child.position, child.rotation)
                    .onOpenTowerSelectPanelEvent += gamePlayUIController.OpenTowerSelectPanel;
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