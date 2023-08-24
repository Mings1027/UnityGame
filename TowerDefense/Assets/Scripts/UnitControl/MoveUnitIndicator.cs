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
        public UnitTower UnitTower { private get; set; }

        [SerializeField] private Image cantMoveImage;

        private void Awake()
        {
            _cam = Camera.main;
            _cantMoveImageSequence = DOTween.Sequence().SetAutoKill(false).Pause()
                .Append(cantMoveImage.transform.DOScale(new Vector3(1, 1), 0.5f).From(0)
                    .SetEase(Ease.OutBounce))
                .Join(cantMoveImage.DOFade(0, 0.5f).From(1));
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
            if (Physics.Raycast(ray, out var physicsHit) && physicsHit.collider.CompareTag("Ground") &&
                Vector3.Distance(transform.position, physicsHit.point) <= transform.localScale.x * 0.5f)
            {
                UnitTower.UnitMove(physicsHit.point);
                UnitTower = null;
                gameObject.SetActive(false);
            }
            else
            {
                cantMoveImage.transform.position = Input.mousePosition;
                _cantMoveImageSequence.Restart();
            }
        }
    }
}