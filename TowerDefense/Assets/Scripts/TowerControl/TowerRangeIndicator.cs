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

        [SerializeField] private DecalProjector projector;

        private void Awake()
        {
            _rangeIndicator = GetComponent<MeshRenderer>();
            _rangeIndicator.transform.localScale = Vector3.zero;
            _rangeTween = transform.DOScale(Vector3.one, 0.2f).SetAutoKill(false).Pause();
            _selectedTween = DOTween.To(() => 0, x => projector.fadeFactor = x, 0.3f, 1)
                .SetLoops(-1, LoopType.Yoyo).SetAutoKill(false).Pause();
            _rangeIndicator.enabled = false;
            projector.enabled = false;
        }

        public void SetIndicator(Vector3 pos, int towerRange)
        {
            _rangeIndicator.enabled = true;
            var r = _rangeIndicator.transform;
            r.position = pos;
            _rangeTween.ChangeStartValue(transform.localScale)
                .ChangeEndValue(new Vector3(towerRange, 0.5f, towerRange))
                .Restart();
            projector.transform.position = pos + Vector3.up * 3;
            projector.enabled = true;
            _selectedTween.Restart();
        }

        public void SetIndicator(Vector3 pos)
        {
            _rangeIndicator.enabled = true;
            var r = _rangeIndicator.transform;
            r.position = pos;
            projector.transform.position = pos + Vector3.up * 3;
            projector.enabled = true;
            _selectedTween.Restart();
        }

        public void DisableIndicator()
        {
            _rangeTween.ChangeEndValue(Vector3.zero).Restart();
            projector.enabled = false;
            _selectedTween.Pause();
        }
    }
}