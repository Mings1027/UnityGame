using System;
using DG.Tweening;
using UnityEngine;

namespace UIControl
{
    public class FollowWorld : MonoBehaviour
    {
        private Camera _cam;
        private Sequence _showSequence, _hideSequence;
        private Transform _target;
        private Vector3 _pos;

        [SerializeField] private Vector3 offset;
        [SerializeField] private Ease scaleEase;
        [SerializeField] private float duration;

        private void Awake()
        {
            gameObject.SetActive(true);
            _cam = Camera.main;
            _showSequence = DOTween.Sequence().SetAutoKill(false).Pause()
                .Append(transform.DOScale(0.5f, duration).From())
                .SetEase(scaleEase);

            _hideSequence = DOTween.Sequence().SetAutoKill(false).Pause()
                .Append(transform.DOScale(0f, duration))
                .SetEase(scaleEase)
                .OnComplete(() => gameObject.SetActive(false));
            gameObject.SetActive(false);
        }

        private void Update()
        {
            _pos = _cam.WorldToScreenPoint(_target.position + offset);
            if (transform.position == _pos) return;
            transform.position = _pos;
        }

        private void OnDestroy()
        {
            _showSequence.Kill();
        }

        public void WorldTarget(Transform t)
        {
            _target = t;
        }

        public void ActivePanel()
        {
            _hideSequence.Pause();
            if (!gameObject.activeSelf)
                gameObject.SetActive(true);
            _showSequence.Restart();
        }

        public void DeActivePanel()
        {
            _showSequence.Pause();
            _hideSequence.Restart();
        }
    }
}