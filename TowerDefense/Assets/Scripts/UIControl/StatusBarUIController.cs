using System.Collections.Generic;
using GameControl;
using ManagerControl;
using StatusControl;
using UnityEngine;

namespace UIControl
{
    public class StatusBarUIController : Singleton<StatusBarUIController>
    {
        private Camera _cam;
        private CameraManager _cameraManager;
        private Dictionary<StatusBar, Transform> _barDictionary;
        private Dictionary<Transform, StatusBar> _inverseDic;

        protected override void Awake()
        {
            _cam = Camera.main;
            _barDictionary = new Dictionary<StatusBar, Transform>();
            _inverseDic = new Dictionary<Transform, StatusBar>();
            enabled = false;
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

        public void Add(StatusBar statusBar, Transform barPosition)
        {
            _barDictionary[statusBar] = barPosition;
            _inverseDic[barPosition] = statusBar;
            _cameraManager.ResizeUI(statusBar.transform);

            statusBar.transform.position = _cam.WorldToScreenPoint(barPosition.position);
        }

        public void Remove(Transform barPosition)
        {
            if (!_inverseDic.ContainsKey(barPosition)) return;
            var key = _inverseDic[barPosition];
            _inverseDic.Remove(barPosition);
            _barDictionary.Remove(key);
            key.RemoveEvent();
            key.gameObject.SetActive(false);
        }
    }
}