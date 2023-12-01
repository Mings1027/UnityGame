using DG.Tweening;
using UnityEngine;

namespace TowerControl
{
    public class TowerRangeIndicator : MonoBehaviour
    {
        private MeshRenderer _rangeIndicator;
        private Tweener _rangeIndicatorTween;

        private void Awake()
        {
            _rangeIndicator = GetComponent<MeshRenderer>();
            _rangeIndicatorTween = _rangeIndicator.transform.DOScale(Vector3.one, 0.15f).SetEase(Ease.OutBack)
                .SetAutoKill(false).Pause();
            _rangeIndicator.transform.localScale = Vector3.zero;
        }

        public void SetIndicator(in Vector3 pos, int towerRange)
        {
            _rangeIndicator.transform.position = pos;
            _rangeIndicatorTween.ChangeStartValue(_rangeIndicator.transform.localScale)
                .ChangeEndValue(new Vector3(towerRange, 0.5f, towerRange)).Restart();
        }

        public void DisableIndicator() => _rangeIndicatorTween.ChangeEndValue(Vector3.zero).Restart();
    }
}