using DG.Tweening;
using GameControl;
using TMPro;
using UnityEngine;

namespace TextControl
{
    public class FloatingText : MonoBehaviour
    {
        private RectTransform _rectTransform;
        private TMP_Text _text;
        private Tweener _upTween;
        private Tweener _alphaTween;
        private Tween _scaleTween;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _text = GetComponent<TMP_Text>();

            _scaleTween = _rectTransform.DOScale(1, 0.5f).From(0).SetEase(Ease.OutBack).SetAutoKill(false).Pause();
            _upTween = _rectTransform.DOAnchorPosY(_rectTransform.localPosition.y, 1.5f).SetAutoKill(false).Pause();
            _alphaTween = _text.DOFade(0, 1.5f).From(1).SetAutoKill(false).Pause();
        }

        private void OnEnable()
        {
            Vector2 position = _rectTransform.localPosition;
            _scaleTween.Restart();
            var randomX = Random.Range(-20, 20);
            _upTween.ChangeStartValue(position + new Vector2(randomX, Random.Range(40, 60)))
                .ChangeEndValue(position + new Vector2(randomX, 100)).Restart();
            _alphaTween.OnComplete(() => gameObject.SetActive(false)).Restart();
        }

        public void SetText(ushort number, bool isGainedCoin = true)
        {
            _text.text = CachedNumber.GetFloatingText(number, isGainedCoin);
        }
    }
}