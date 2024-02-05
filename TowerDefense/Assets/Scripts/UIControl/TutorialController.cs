using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace UIControl
{
    public class TutorialController : MonoBehaviour
    {
        private Sequence _bounceSequence;
        private RectTransform _rectTransform;
        [SerializeField] private Ease ease;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _bounceSequence = DOTween.Sequence().SetAutoKill(false).Pause()
                .Append(_rectTransform.DOAnchorPosY(70, 0.7f).SetEase(ease));
            var toggleBtn = _rectTransform.GetComponent<Button>();
            toggleBtn.onClick.AddListener(BounceButton);
            toggleBtn.enabled = false;
        }

        private void Start()
        {
            gameObject.SetActive(false);
        }

        private void BounceButton()
        {
            _rectTransform.anchoredPosition = Vector2.zero;
            _bounceSequence.Kill();
            _rectTransform.GetComponent<Button>().onClick.RemoveListener(BounceButton);
            Destroy(GetComponent<TutorialController>());
        }

        public void TutorialButton()
        {
            gameObject.SetActive(true);
            _rectTransform.GetComponent<Button>().enabled = true;
            _bounceSequence.SetLoops(-1, LoopType.Yoyo).Restart();
        }
    }
}