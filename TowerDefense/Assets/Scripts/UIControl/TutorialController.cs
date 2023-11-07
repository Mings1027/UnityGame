using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace UIControl
{
    public class TutorialController : MonoBehaviour
    {
        private Sequence _bounceSequence;

        [SerializeField] private RectTransform toggleTowerButton;
        [SerializeField] private Ease ease;

        private void Awake()
        {
            _bounceSequence = DOTween.Sequence().SetAutoKill(false).Pause()
                .Append(toggleTowerButton.DOAnchorPosX(70, 1f).SetEase(ease));
            toggleTowerButton.GetComponent<Button>().onClick.AddListener(BounceButton);
        }

        private void Start()
        {
            _bounceSequence.SetLoops(-1, LoopType.Yoyo).Restart();
        }

        private void BounceButton()
        {
            toggleTowerButton.anchoredPosition = Vector2.zero;
            _bounceSequence.Kill();
            toggleTowerButton.GetComponent<Button>().onClick.RemoveListener(BounceButton);
            Destroy(gameObject);
        }
    }
}