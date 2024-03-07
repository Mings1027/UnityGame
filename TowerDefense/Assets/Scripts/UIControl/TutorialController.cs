using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace UIControl
{
    public class TutorialController : MonoBehaviour
    {
        private Vector2 _initPos;
        private Sequence _bounceSequence;
        private RectTransform _rectTransform;
        [SerializeField] private Ease ease;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _initPos = _rectTransform.anchoredPosition;
            _bounceSequence = DOTween.Sequence().SetAutoKill(false).Pause()
                .Append(_rectTransform.DOAnchorPosX(70, 0.7f).SetEase(ease));
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
            _rectTransform.anchoredPosition = _initPos;
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