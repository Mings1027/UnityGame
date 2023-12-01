using DG.Tweening;
using ManagerControl;
using TowerControl;
using UIControl;
using UnityEngine;

namespace UnitControl.TowerUnitControl
{
    public class MoveUnitController : MonoBehaviour
    {
        private Camera _cam;
        private CameraManager _cameraManager;
        private UnitTower _unitTower;
        private Tweener _camMoveTween;
        private Tweener _camZoomTween;

        private float _prevSize;
        private Vector3 _prevPos;

        [SerializeField] private float camZoomTime;

        private void Awake()
        {
            _cam = Camera.main;
        }

        private void Start()
        {
            _cameraManager = _cam.GetComponentInParent<CameraManager>();
            _camMoveTween = _cameraManager.transform.DOMove(_cameraManager.transform.position, camZoomTime)
                .SetAutoKill(false).Pause();
            _camZoomTween = _cam.DOOrthoSize(10, camZoomTime).SetAutoKill(false).Pause();
            enabled = false;
        }

        private void Update()
        {
            if (Input.touchCount <= 0) return;
            CheckMoveUnit();
        }

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
    }
}