using System;
using DG.Tweening;
using UnityEngine;

namespace GameControl
{
    public class SetActiveOfPoolObject : MonoBehaviour
    {
        [SerializeField] private float lifeTime;
        private Tween _destroyTween;
        
        
        private void Awake()
        {
            _destroyTween = DOVirtual.DelayedCall(lifeTime, DestroyObject).SetAutoKill(false);
        }
        private void OnEnable()
        {
            _destroyTween.Restart();
        }

        private void OnDisable()
        {
            StackObjectPool.ReturnToPool(gameObject);
        }

        private void OnDestroy()
        {
            _destroyTween.Kill();
        }

        private void DestroyObject()
        {
            gameObject.SetActive(false);
        }
    }
}
