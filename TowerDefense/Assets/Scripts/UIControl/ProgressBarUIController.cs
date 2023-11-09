using System.Collections.Generic;
using ManagerControl;
using UnityEngine;
using ProgressBar = StatusControl.ProgressBar;

namespace UIControl
{
    public class ProgressBarUIController : MonoBehaviour
    {
        private static ProgressBarUIController _inst;
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

        private void Update()
        {
            if (Input.touchCount > 0)
            {
                foreach (var pro in _progressBarDictionary.Keys)
                {
                    pro.transform.position = _cam.WorldToScreenPoint(_progressBarDictionary[pro].position)
                                             + (Vector3)Input.GetTouch(0).deltaPosition.normalized;
                }
            }
            else
            {
                foreach (var pro in _progressBarDictionary.Keys)
                {
                    pro.transform.position = _cam.WorldToScreenPoint(_progressBarDictionary[pro].position);
                }
            }
        }

        private void ResizeUI()
        {
            foreach (var key in _inverseDic.Keys)
            {
                _cameraManager.ResizeUIElement(_inverseDic[key].transform);
            }
        }

        private void SetProgressBar(ProgressBar progressBar, Transform barPosition)
        {
            _progressBarDictionary[progressBar] = barPosition;
            _inverseDic[barPosition] = progressBar;
            _cameraManager.ResizeUIElement(progressBar.transform);

            progressBar.transform.position = _cam.WorldToScreenPoint(barPosition.position);
        }

        private void RemoveProgressBar(Transform barPosition)
        {
            if (!_inverseDic.ContainsKey(barPosition)) return;

            var key = _inverseDic[barPosition];
            _inverseDic.Remove(barPosition);
            _progressBarDictionary.Remove(key);
        }

        public static void Add(ProgressBar progressBar, Transform barPosition)
        {
            _inst.SetProgressBar(progressBar, barPosition);
        }

        public static void Remove(Transform barPosition)
        {
            _inst.RemoveProgressBar(barPosition);
        }
    }
}