using DG.Tweening;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TextControl
{
    public class FloatingText : MonoBehaviour
    {
        private RectTransform _rectTransform;
        private TMP_Text _text;
        private Sequence _scaleAlphaSequence;
        private Tweener _upTween;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _text = GetComponent<TMP_Text>();

            _scaleAlphaSequence = DOTween.Sequence().SetAutoKill(false).Pause()
                .Append(_rectTransform.DOScale(1, 0.5f).From(0).SetEase(Ease.OutBack))
                .Join(_text.DOFade(0, 1.5f).From(1));
            _upTween = _rectTransform.DOAnchorPosY(_rectTransform.localPosition.y, 1.5f).SetAutoKill(false).Pause();
        }

        private void OnEnable()
        {
            Vector2 position = _rectTransform.localPosition;
            var randomX = Random.Range(-20, 20);
            var startPos = position + new Vector2(randomX, Random.Range(40, 100));
            var endPos = startPos + new Vector2(randomX, 100);
            _upTween.ChangeStartValue(startPos).ChangeEndValue(endPos).Restart();
            _scaleAlphaSequence.OnComplete(() => gameObject.SetActive(false)).Restart();
        }

        private void OnDestroy()
        {
            _scaleAlphaSequence?.Kill();
            _upTween?.Kill();
        }

        public void SetCostText(ushort number, bool isPlusValue = true)
        {
            _text.text = isPlusValue
                ? "+" + number + "g"
                : "-" + number + "g";
        }

        public void SetHpText(ushort number, bool isPlusValue = true)
        {
            _text.text = isPlusValue
                ? "+" + number + " HP"
                : "-" + number + " HP";
        }
    }
}