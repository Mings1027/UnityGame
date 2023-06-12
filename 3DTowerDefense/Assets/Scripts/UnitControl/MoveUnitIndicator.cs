using System;
using DG.Tweening;
using TowerControl;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

namespace UnitControl
{
    public class MoveUnitIndicator : MonoBehaviour
    {
        private Camera _cam;
        private Sequence _cantMoveImageSequence;

        public event Action onMoveUnitEvent;
        public BarracksUnitTower BarracksTower { get; set; }

        [SerializeField] private GameObject cantMoveImage;

        private void Awake()
        {
            _cam = Camera.main;
            _cantMoveImageSequence = DOTween.Sequence().SetAutoKill(false).Pause()
                .Append(cantMoveImage.transform.DOScale(new Vector3(1, 1), 0.5f).From(0.5f).SetEase(Ease.OutBounce))
                .Join(cantMoveImage.GetComponent<Image>().DOFade(0, 1f).From(1));
        }

        private void Update()
        {
            if (Input.touchCount <= 0) return;
            var touch = Input.GetTouch(0);
            if (touch.phase != TouchPhase.Ended) return;

            CheckCanMoveUnit(touch);
        }

        private void CheckCanMoveUnit(Touch touch)
        {
            var ray = _cam.ScreenPointToRay(touch.position);
            Physics.Raycast(ray, out var physicsHit);

            if (NavMesh.SamplePosition(physicsHit.point, out var unitCenterPos, 1, NavMesh.AllAreas)
                && Vector3.Distance(transform.position, physicsHit.point) < transform.localScale.x * 0.5f)
            {
                BarracksTower.UnitMove(unitCenterPos.position);
                onMoveUnitEvent?.Invoke();
                gameObject.SetActive(false);
                cantMoveImage.SetActive(false);
            }
            else
            {
                cantMoveImage.transform.position = _cam.WorldToScreenPoint(physicsHit.point);
                _cantMoveImageSequence.Restart();
                if (cantMoveImage.activeSelf) return;
                cantMoveImage.SetActive(true);
            }
        }
    }
}