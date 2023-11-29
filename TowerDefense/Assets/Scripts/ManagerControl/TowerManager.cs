using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using PoolObjectControl;
using TowerControl;
using UIControl;
using Unity.VisualScripting;
using UnityEngine;

namespace ManagerControl
{
    public class TowerManager : MonoBehaviour
    {
        private static TowerManager _towerManager;
        private CancellationTokenSource _cts;
        private Camera _cam;
        private CameraManager _cameraManager;
        private List<Tower> _towers;
        private UnitTower _unitTower;
        private Tweener _camMoveTween;
        private Tweener _camZoomTween;

        private float _prevSize;
        private Vector3 _prevPos;

        [Header("----------Indicator----------"), SerializeField]
        private MeshRenderer rangeIndicator;

        [SerializeField] private float camZoomTime;

        #region Unity Event

        protected void Awake()
        {
            _towerManager = this;
            _cam = Camera.main;
            _towers = new List<Tower>(50);
        }

        private void Start()
        {
            _cameraManager = _cam.GetComponentInParent<CameraManager>();
            _camMoveTween = _cameraManager.transform.DOMove(_cameraManager.transform.position, camZoomTime)
                .SetAutoKill(false).Pause();
            _camZoomTween = _cam.DOOrthoSize(10, camZoomTime).SetAutoKill(false).Pause();
            IndicatorInit();
            enabled = false;
        }

        private void Update()
        {
            if (Input.touchCount <= 0) return;
            CheckMoveUnit();
        }

        private void OnDestroy()
        {
            _cts?.Cancel();
            _cts?.Dispose();
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            Application.targetFrameRate = pauseStatus ? 20 : 90;
        }

        #endregion

        #region TowerControl

        public void StartTargeting()
        {
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
            Application.targetFrameRate = 90;
            TargetingAsync().Forget();
        }

        public void StopTargeting()
        {
            _cts?.Cancel();

            TargetInit();
            SlowDownFrameRate().Forget();
            PoolObjectManager.PoolCleaner().Forget();
        }

        private async UniTaskVoid TargetingAsync()
        {
            while (!_cts.IsCancellationRequested)
            {
                await UniTask.Delay(10, cancellationToken: _cts.Token);
                if (Time.timeScale == 0) continue;

                var towerCount = _towers.Count;
                for (var i = 0; i < towerCount; i++)
                {
                    _towers[i].TowerUpdate(_cts);
                }
            }
        }

        private void TargetInit()
        {
            var towerCount = _towers.Count;
            for (var i = 0; i < towerCount; i++)
            {
                _towers[i].TowerTargetInit();
            }
        }

        private async UniTaskVoid SlowDownFrameRate()
        {
            var frameRate = 60;
            while (frameRate > 45)
            {
                frameRate -= 1;
                Application.targetFrameRate = frameRate;
                await UniTask.Delay(1000, cancellationToken: _cts.Token);
            }
        }

        #endregion

        private void IndicatorInit()
        {
            rangeIndicator.transform.localScale = Vector3.zero;
        }

        #region Private Method

        public void FocusUnitTower(UnitTower unitTower)
        {
            enabled = true;
            _unitTower = unitTower;
            _prevSize = _cam.orthographicSize;
            _prevPos = _cameraManager.transform.position;
            _camMoveTween.ChangeStartValue(_prevPos).ChangeEndValue(_unitTower.transform.position).Restart();
            _camZoomTween.ChangeStartValue(_prevSize).ChangeEndValue(10f).Restart();
        }

        private void RewindCam()
        {
            enabled = false;
            _camMoveTween.ChangeStartValue(_cameraManager.transform.position).ChangeEndValue(_prevPos).Restart();
            _camZoomTween.ChangeStartValue(_cam.orthographicSize).ChangeEndValue(_prevSize).Restart();
        }

        private void CheckMoveUnit()
        {
            var touch = Input.GetTouch(0);
            if (!touch.deltaPosition.Equals(Vector2.zero)) return;
            var ray = _cam.ScreenPointToRay(Input.mousePosition);
            Physics.Raycast(ray, out var hit);
            if (hit.collider && hit.collider.CompareTag("Ground") &&
                Vector3.Distance(_unitTower.transform.position, hit.point) <= _unitTower.TowerRange)
            {
                _unitTower.UnitMove(new Vector3(hit.point.x, 0, hit.point.z));
                RewindCam();
                UIManager.Instance.OffUI();
            }
            else
            {
                UIManager.Instance.YouCannotMove();
            }
        }

        #endregion

        #region Public Method

        public void AddTower(Tower tower)
        {
            _towers.Add(tower);
        }

        public void RemoveTower(Tower tower)
        {
            _towers.Remove(tower);
        }

        public static async UniTaskVoid DisableProjectile(GameObject g)
        {
            var time = 2f;
            while (time > 0 && !ReferenceEquals(_towerManager, null))
            {
                time -= Time.deltaTime;
                await UniTask.Yield();
            }

            g.SetActive(false);
        }

        #endregion
    }
}