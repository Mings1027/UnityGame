using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using ManagerControl;
using TowerControl;
using UnityEngine;
using UnityEngine.UI;

namespace UnitControl
{
    public class MoveUnitIndicator : MonoBehaviour
    {
        private RaycastHit physicsHit;
        private TowerManager _towerManager;
        private Camera _cam;
        private Sequence _cantMoveImageSequence;
        public UnitTower UnitTower { private get; set; }
        public float distance;

        [SerializeField] private Image cantMoveImage;
        [SerializeField] private MeshRenderer unitCenterIndicator;

        private void Awake()
        {
            _towerManager = TowerManager.Instance;
            _cam = Camera.main;
            _cantMoveImageSequence = DOTween.Sequence().SetAutoKill(false).Pause()
                .Append(cantMoveImage.transform.DOScale(new Vector3(1, 1), 0.5f).From(0)
                    .SetEase(Ease.OutBounce))
                .Join(cantMoveImage.DOFade(0, 0.5f).From(1));
            unitCenterIndicator.enabled = false;
        }

        private void OnEnable()
        {
            if (UnitTower == null) return;
            unitCenterIndicator.enabled = true;
            unitCenterIndicator.transform.position = UnitTower.unitCenterPos;
        }

        private void Update()
        {
            if (Input.touchCount <= 0) return;
            var touch = Input.GetTouch(0);
            if (touch.phase != TouchPhase.Ended) return;
            if (touch.deltaPosition != Vector2.zero) return;

            CheckCanMoveUnit(touch);
        }

        private void OnDestroy()
        {
            _cantMoveImageSequence.Kill();
        }

        private void CheckCanMoveUnit(Touch touch)
        {
            var ray = _cam.ScreenPointToRay(touch.position);
            if (Physics.Raycast(ray, out physicsHit) && physicsHit.collider.CompareTag("Ground") &&
                Vector3.Distance(UnitTower.transform.position, physicsHit.point) <= distance)
            {
                _towerManager.RewindCamState();
                _towerManager.ResetUI();
                UnitTower.UnitMove(physicsHit.point);
                UnitTower = null;
                unitCenterIndicator.transform.position = physicsHit.point;
                NotifyDestination().Forget();
                enabled = false;
            }
            else
            {
                cantMoveImage.transform.position = Input.mousePosition;
                _cantMoveImageSequence.Restart();
            }
        }

        private async UniTaskVoid NotifyDestination()
        {
            await unitCenterIndicator.transform.DOScale(0.5f, 0.4f).SetLoops(4, LoopType.Yoyo);
            unitCenterIndicator.enabled = false;
        }
    }
}