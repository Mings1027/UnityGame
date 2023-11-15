using System.Collections.Generic;
using GameControl;
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
        private SerializableDictionary<ProgressBar, Transform> _barDictionary;
        private SerializableDictionary<Transform, ProgressBar> _inverseDic;

        private void Awake()
        {
            _inst = this;
            _cam = Camera.main;
            _barDictionary = new SerializableDictionary<ProgressBar, Transform>();
            _inverseDic = new SerializableDictionary<Transform, ProgressBar>();
        }

        private void Start()
        {
            _cameraManager = _cam.GetComponentInParent<CameraManager>();
            _cameraManager.OnResizeUIEvent += ResizeUI;
        }

        private void Update()
        {
            if (Input.touchCount > 0)
            {
                foreach (var bar in _barDictionary.Keys)
                {
                    bar.transform.position = _cam.WorldToScreenPoint(_barDictionary[bar].position)
                                             + (Vector3)Input.GetTouch(0).deltaPosition.normalized;
                }
            }
            else
            {
                foreach (var bar in _barDictionary.Keys)
                {
                    bar.transform.position = _cam.WorldToScreenPoint(_barDictionary[bar].position);
                }
            }
        }

        private void ResizeUI()
        {
            foreach (var key in _inverseDic.Keys)
            {
                _cameraManager.ResizeUI(_inverseDic[key].transform);
            }
        }

        private void SetProgressBar(ProgressBar progressBar, Transform barPosition)
        {
            _barDictionary[progressBar] = barPosition;
            _inverseDic[barPosition] = progressBar;
            _cameraManager.ResizeUI(progressBar.transform);

            progressBar.transform.position = _cam.WorldToScreenPoint(barPosition.position);
        }

        private void RemoveProgressBar(Transform barPosition)
        {
            if (!_inverseDic.ContainsKey(barPosition)) return;

            var key = _inverseDic[barPosition];
            _inverseDic.Remove(barPosition);
            _barDictionary.Remove(key);
            key.RemoveEvent();
            key.gameObject.SetActive(false);
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