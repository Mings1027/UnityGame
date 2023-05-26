using System;
using BuildControl;
using DataControl;
using DG.Tweening;
using GameControl;
using ManagerControl;
using UIControl;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MapControl
{
    public class MapController : MonoBehaviour
    {
        [SerializeField] private Transform towerBuildPoint;

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
                b.onClickBuildPointEvent += () => gameManager.IsClickBuildPoint = true;
                b.onOpenTowerSelectPanelEvent += gamePlayUIController.OpenTowerSelectPanel;
            }
        }
    }
}