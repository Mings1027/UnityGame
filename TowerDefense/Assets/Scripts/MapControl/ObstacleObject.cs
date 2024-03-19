using DG.Tweening;
using UnityEngine;

namespace MapControl
{
    public abstract class ObstacleObject : MonoBehaviour
    {
        private Rigidbody _rigid;

        protected virtual void Awake()
        {
            _rigid = GetComponent<Rigidbody>();
        }

        public void FloatObstacle()
        {
            _rigid.DOMoveY(1, 1).From(-5).SetEase(Ease.OutBack);
        }
    }
}