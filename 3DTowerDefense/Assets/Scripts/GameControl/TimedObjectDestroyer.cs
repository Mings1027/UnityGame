using System;
using DG.Tweening;
using UnityEngine;

namespace GameControl
{
    public class TimedObjectDestroyer : MonoBehaviour
    {
        private Tween disableTween;

        [SerializeField] private float lifeTime;

        private void Awake()
        {
            disableTween = DOVirtual.DelayedCall(lifeTime, () => gameObject.SetActive(false))
                .SetAutoKill(false);
        }

        private void OnEnable()
        {
            disableTween.Restart();
        }
    }
}