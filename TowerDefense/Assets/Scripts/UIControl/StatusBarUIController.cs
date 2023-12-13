using System.Collections.Generic;
using ManagerControl;
using StatusControl;
using UnityEngine;

namespace UIControl
{
    public class StatusBarUIController : MonoBehaviour
    {
        private static StatusBarUIController _inst;
        private Camera _cam;
        private CameraManager _cameraManager;
        private Dictionary<StatusBar, Transform> _barDictionary;
        private Dictionary<Transform, StatusBar> _inverseDic;

        protected void Awake()
        {
            _inst = this;
            _cam = Camera.main;
            _barDictionary = new Dictionary<StatusBar, Transform>();
            _inverseDic = new Dictionary<Transform, StatusBar>();
            // enabled = false;
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

        public static void Add(StatusBar statusBar, Transform barPosition)
        {
            _inst._barDictionary[statusBar] = barPosition;
            _inst._inverseDic[barPosition] = statusBar;
            _inst._cameraManager.ResizeUI(statusBar.transform);

            statusBar.transform.position = _inst._cam.WorldToScreenPoint(barPosition.position);
        }

        public static void Remove(Transform barPosition, bool removeDirectly = false)
        {
            if (!_inst._inverseDic.ContainsKey(barPosition)) return;
            var key = _inst._inverseDic[barPosition];
            _inst._inverseDic.Remove(barPosition);
            _inst._barDictionary.Remove(key);
            key.RemoveEvent(removeDirectly);
        }
    }
}