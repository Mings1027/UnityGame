using Cysharp.Threading.Tasks;
using DG.Tweening;
using UniTaskTweenControl;
using UnityEngine;

namespace TowerControl
{
    public class TowerRangeIndicator : MonoBehaviour
    {
        private MeshRenderer _rangeIndicator;
        private Tweener _rangeTween;

        private void Awake()
        {
            _rangeIndicator = GetComponent<MeshRenderer>();
            _rangeIndicator.transform.localScale = Vector3.zero;
            _rangeTween = transform.DOScale(Vector3.one, 0.2f).SetAutoKill(false).Pause();
        }

        public void SetIndicator(Vector3 pos, int towerRange)
        {
            _rangeIndicator.enabled = true;
            var r = _rangeIndicator.transform;
            r.position = pos;
            _rangeTween.ChangeStartValue(transform.localScale)
                .ChangeEndValue(new Vector3(towerRange, 0.5f, towerRange))
                .Restart();
        }

        public void DisableIndicator()
        {
            _rangeTween.ChangeEndValue(Vector3.zero).Restart();
        }
    }
}