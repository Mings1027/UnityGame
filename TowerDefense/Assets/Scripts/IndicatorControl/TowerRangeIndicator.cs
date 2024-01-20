using DG.Tweening;
using UnityEngine;

namespace IndicatorControl
{
    public class TowerRangeIndicator : MonoBehaviour
    {
        private MeshRenderer _rangeIndicator;
        private Tweener _rangeTween;
        private Tweener _selectedTween;

        private void Awake()
        {
            _rangeIndicator = GetComponent<MeshRenderer>();
            _rangeIndicator.transform.localScale = Vector3.zero;
            _rangeTween = transform.DOScale(Vector3.one, 0.2f).SetAutoKill(false).Pause();
        }

        public void SetIndicator(Vector3 pos, int towerRange)
        {
            _rangeIndicator.enabled = true;
            _rangeIndicator.transform.position = pos;
            _rangeTween.ChangeStartValue(transform.localScale)
                .ChangeEndValue(new Vector3(towerRange, 0.5f, towerRange)).Restart();
        }

        public void DisableIndicator()
        {
            _rangeTween.ChangeEndValue(Vector3.zero).Restart();
            _selectedTween.Pause();
        }
    }
}