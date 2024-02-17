using ManagerControl;
using TowerControl;
using UniTaskTweenControl;
using UnityEngine;

namespace UnitControl
{
    public class MoveUnitController : MonoBehaviour
    {
        private UIManager _uiManager;
        private Camera _cam;
        private Transform _camArm;
        private SummonTower _summonTower;

        private float _prevSize;
        private Vector3 _prevPos;

        [SerializeField] private float camZoomTime;
        [SerializeField] private LayerMask unitLayer;

        private void Awake()
        {
            _cam = Camera.main;
        }

        private void Start()
        {
            _uiManager = UIManager.instance;
            _camArm = _cam.transform.parent;
            enabled = false;
        }

        private void Update()
        {
            if (Input.touchCount <= 0) return;
            CheckMoveUnit();
        }

        public void FocusUnitTower(SummonTower summonTower)
        {
            enabled = true;
            _summonTower = summonTower;
            _prevSize = _cam.orthographicSize;
            _prevPos = _camArm.position;
            _camArm.MoveTween(_prevPos, summonTower.transform.position, camZoomTime);
            _cam.OrthoSizeTween(_prevSize, 10, camZoomTime);
        }

        private void RewindCam()
        {
            enabled = false;
            _camArm.MoveTween(_camArm.position, _prevPos, camZoomTime);
            _cam.OrthoSizeTween(_cam.orthographicSize, _prevSize, camZoomTime);
        }

        private void CheckMoveUnit()
        {
            var touch = Input.GetTouch(0);
            if (!touch.deltaPosition.Equals(Vector2.zero)) return;
            var ray = _cam.ScreenPointToRay(Input.mousePosition);
            Physics.Raycast(ray, out var hit, int.MaxValue);
            if (hit.collider && hit.collider.CompareTag("Ground") &&
                Vector3.Distance(_summonTower.transform.position, hit.point) <= _summonTower.TowerRange)
            {
                _summonTower.UnitMove(new Vector3(hit.point.x, 0, hit.point.z));
                RewindCam();
                _uiManager.OffUI();
            }
            else
            {
                _uiManager.UnitCannotMove();
            }
        }
    }
}