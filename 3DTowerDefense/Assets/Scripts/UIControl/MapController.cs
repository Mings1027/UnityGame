using System;
using BuildControl;
using DG.Tweening;
using GameControl;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UIControl
{
    public class MapController : Singleton<MapController>, IPointerDownHandler, IPointerUpHandler
    {
        private UIManager _uiManager;

        public Sequence BuildPointSequence { get; private set; }

        private void Awake()
        {
            _uiManager = UIManager.Instance;
        }

        private void Start()
        {
            BuildPointSequence = DOTween.Sequence().SetAutoKill(false).Pause();
            var buildPoint = transform.GetChild(0);
            for (var i = 0; i < buildPoint.childCount; i++)
            {
                var child = buildPoint.GetChild(i);
                var p = StackObjectPool.Get<BuildingPoint>("BuildingPoint", child.position + new Vector3(0, 300, 0),
                    child.rotation);
                p.onOpenTowerSelectPanelEvent += _uiManager.OpenTowerSelectPanel;
                BuildPointSequence.Append(p.transform.DOMoveY(0, 0.2f)
                    .OnComplete(() => StackObjectPool.Get("BuildSmoke", p.transform.position)));
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (_uiManager.IsMoveUnit) return;
            _uiManager.CloseUI();
        }
    }
}