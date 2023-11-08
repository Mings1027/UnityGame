using System.Collections.Generic;
using ManagerControl;
using UnityEngine;
using ProgressBar = StatusControl.ProgressBar;

namespace UIControl
{
    public class ProgressBarCanvasController : MonoBehaviour
    {
        private static ProgressBarCanvasController _inst;
        private Camera _cam;
        private CameraManager _cameraManager;
        private Dictionary<ProgressBar, Transform> _progressBarDictionary;
        private Dictionary<Transform, ProgressBar> _inverseDic;

        private void Awake()
        {
            _inst = this;
            _cam = Camera.main;
            _progressBarDictionary = new Dictionary<ProgressBar, Transform>();
            _inverseDic = new Dictionary<Transform, ProgressBar>();
        }

        private void Start()
        {
            _cameraManager = _cam.GetComponentInParent<CameraManager>();
            _cameraManager.OnHealthBarZoomEvent += ResizeUI;
        }

        private void LateUpdate()
        {
            foreach (var pro in _progressBarDictionary.Keys)
            {
                pro.transform.position = _cam.WorldToScreenPoint(_progressBarDictionary[pro].position);
            }
        }

        private void ResizeUI()
        {
            foreach (var key in _inverseDic.Keys)
            {
                _cameraManager.ResizeUIElement(_inverseDic[key].transform);
            }
        }

        public static void AddProgressBar(ProgressBar progressBar, Transform progressBarPosition)
        {
            _inst._progressBarDictionary[progressBar] = progressBarPosition;
            _inst._inverseDic[progressBarPosition] = progressBar;
            _inst._cameraManager.ResizeUIElement(progressBar.transform);
        }

        public static void RemoveProgressBar(Transform progressBarPosition)
        {
            if (!_inst._inverseDic.ContainsKey(progressBarPosition)) return;
            var key = _inst._inverseDic[progressBarPosition];
            _inst._inverseDic.Remove(progressBarPosition);
            _inst._progressBarDictionary.Remove(key);
        }
    }
}