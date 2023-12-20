using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace TowerControl
{
    public class TowerRangeIndicator : MonoBehaviour
    {
        private MeshRenderer _rangeIndicator;
        private Tweener _rangeTween;
        private Tweener _selectedTween;

        [SerializeField] private MeshRenderer selectTowerIndicator;

        private void Awake()
        {
            _rangeIndicator = GetComponent<MeshRenderer>();
            _rangeIndicator.transform.localScale = Vector3.zero;
            _rangeTween = transform.DOScale(Vector3.one, 0.2f).SetAutoKill(false).Pause();
            _selectedTween = selectTowerIndicator.transform.DOMoveY(2.5f, 1)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo).SetAutoKill(false).Pause();
            _rangeIndicator.enabled = false;
            selectTowerIndicator.enabled = false;
        }

        public void SetIndicator(Vector3 pos, int towerRange)
        {
            _rangeIndicator.enabled = true;
            selectTowerIndicator.enabled = true;
            _rangeIndicator.transform.position = pos;
            selectTowerIndicator.transform.position = pos + new Vector3(0, 1, 0);
            _rangeTween.ChangeStartValue(transform.localScale)
                .ChangeEndValue(new Vector3(towerRange, 0.5f, towerRange)).Restart();
            _selectedTween.ChangeStartValue(selectTowerIndicator.transform.position).Restart();
        }

        public void SetIndicator(Vector3 pos)
        {
            _rangeIndicator.enabled = true;
            selectTowerIndicator.enabled = true;
            _rangeIndicator.transform.position = pos;
            selectTowerIndicator.transform.position = pos + new Vector3(0, 1, 0);
            _selectedTween.ChangeStartValue(selectTowerIndicator.transform.position).Restart();
        }

        public void DisableIndicator()
        {
            _rangeTween.ChangeEndValue(Vector3.zero).Restart();
            selectTowerIndicator.enabled = false;
            _selectedTween.Pause();
        }
    }
}