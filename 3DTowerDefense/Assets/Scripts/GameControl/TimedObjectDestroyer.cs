// using System;
// using DG.Tweening;

using UnityEngine;

namespace GameControl
{
    public class TimedObjectDestroyer : MonoBehaviour
    {
        // private Tween _disableTween;

        [SerializeField] private float lifeTime;

        // private void Awake()
        // {
        //     _disableTween = DOVirtual.DelayedCall(lifeTime, () => gameObject.SetActive(false), false)
        //         .SetAutoKill(false);
        // }

        private void OnEnable()
        {
            Invoke(nameof(DestroyObject), lifeTime);
            // _disableTween.Restart();
        }

        private void OnDisable()
        {
            CancelInvoke();
        }

        private void DestroyObject()
        {
            gameObject.SetActive(false);
        }
    }
}