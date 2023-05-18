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
    public class MapController : Singleton<MapController>, IPointerDownHandler, IPointerUpHandler
    {
        private Sequence _buildPointSequence;

        public event Action onCloseUIEvent;

        [SerializeField] private Transform towerBuildPoint;

        private void Start()
        {
            var gamePlayUIController = GamePlayUIController.Instance;
            _buildPointSequence = DOTween.Sequence().SetAutoKill(false).Pause();
            for (int i = 0; i < towerBuildPoint.childCount; i++)
            {
                var child = towerBuildPoint.GetChild(i);
                var p = StackObjectPool.Get<BuildingPoint>("BuildingPoint", child.position + new Vector3(0, 300, 0),
                    child.rotation);
                p.onOpenTowerSelectPanelEvent += gamePlayUIController.OpenTowerSelectPanel;
                _buildPointSequence.Append(p.transform.DOMoveY(0, 0.2f)
                    .OnComplete(() => StackObjectPool.Get("BuildSmoke", p.transform.position)));
            }

            UIManager.Instance.onBuildPointSequenceEvent += () => _buildPointSequence.Restart();
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