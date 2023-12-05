using Cysharp.Threading.Tasks;
using DG.Tweening;
using UniTaskTweenControl;
using UnityEngine;

namespace TowerControl
{
    public class TowerRangeIndicator : MonoBehaviour
    {
        private MeshRenderer _rangeIndicator;

        private void Awake()
        {
            _rangeIndicator = GetComponent<MeshRenderer>();
            _rangeIndicator.transform.localScale = Vector3.zero;
        }

        public void SetIndicator(Vector3 pos, int towerRange)
        {
            _rangeIndicator.enabled = true;
            var r = _rangeIndicator.transform;
            r.position = pos;
            var targetScale = new Vector3(towerRange, 0.5f, towerRange);
            _rangeIndicator.transform.ScaleTween(targetScale, 0.2f);
        }

        public void DisableIndicator()
        {
            _rangeIndicator.transform.ScaleTween(Vector3.zero, 0.2f, () => _rangeIndicator.enabled = false);
        }
    }
}