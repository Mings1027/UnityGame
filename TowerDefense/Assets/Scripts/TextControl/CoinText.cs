using DG.Tweening;
using GameControl;
using TMPro;
using UnityEngine;

namespace TextControl
{
    public class CoinText : MonoBehaviour
    {
        private RectTransform _rectTransform;
        private TMP_Text _text;
        private Tweener _upTween;
        private Tweener _alphaTween;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _text = GetComponent<TMP_Text>();

            _upTween = _rectTransform.DOAnchorPosY(_rectTransform.localPosition.y, 1.5f).SetAutoKill(false).Pause();
            _alphaTween = _text.DOFade(0, 1.5f).From(1).SetAutoKill(false).Pause();
        }

        private void OnEnable()
        {
            Vector2 position = _rectTransform.localPosition;
            _upTween.ChangeStartValue(position)
                .ChangeEndValue(position + new Vector2(0, 50))
                .Restart();
            _alphaTween.OnComplete(() => gameObject.SetActive(false)).Restart();
        }

        public void SetText(int number)
        {
            _text.text = CachedNumber.GetFloatingText(number);
        }
    }
}