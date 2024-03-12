using System.Collections.Generic;
using GameControl;
using ManagerControl;
using StatusControl;
using UnityEngine;

namespace UIControl
{
    public class StatusBarUIController : MonoSingleton<StatusBarUIController>
    {
        private Camera _cam;
        private CameraManager _cameraManager;
        private Dictionary<StatusBar, Transform> _barDictionary;
        private Dictionary<Transform, StatusBar> _inverseDic;

        protected override void Awake()
        {
            base.Awake();
            _cam = Camera.main;
            _barDictionary = new Dictionary<StatusBar, Transform>();
            _inverseDic = new Dictionary<Transform, StatusBar>();
        }

        private void Start()
        {
            _cameraManager = _cam.GetComponentInParent<CameraManager>();
            _cameraManager.OnResizeUIEvent += ResizeUI;
        }

        private void LateUpdate()
        {
            foreach (var bar in _barDictionary.Keys)
            {
                var viewportPos = _cam.WorldToViewportPoint(_barDictionary[bar].position);
                if (viewportPos.x is < -0.1f or > 1.1f || viewportPos.y is < -0.1f or > 1.1f) continue;
                bar.transform.position = _cam.WorldToScreenPoint(_barDictionary[bar].position);
            }
        }

        private void ResizeUI()
        {
            foreach (var key in _inverseDic.Keys)
            {
                _cameraManager.ResizeUI(_inverseDic[key].transform);
            }
        }

        public static void Add(StatusBar statusBar, Transform barPosition)
        {
            instance.AddBar(statusBar, barPosition);
        }

        public static void Remove(Transform barPosition, bool removeDirectly = false)
        {
            instance.RemoveBar(barPosition, removeDirectly);
        }

        private void AddBar(StatusBar statusBar, Transform barPosition)
        {
            _barDictionary[statusBar] = barPosition;
            _inverseDic[barPosition] = statusBar;
            _cameraManager.ResizeUI(statusBar.transform);
            statusBar.transform.position = _cam.WorldToScreenPoint(barPosition.position);
        }

        private void RemoveBar(Transform barPosition, bool removeDirectly = false)
        {
            if (!_inverseDic.ContainsKey(barPosition)) return;
            var key = _inverseDic[barPosition];
            _inverseDic.Remove(barPosition);
            _barDictionary.Remove(key);
            key.RemoveEvent(removeDirectly);
        }
    }
}