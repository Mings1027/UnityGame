using System;
using GameControl;
using UnityEngine;

namespace EffectControl
{
    public class SmokeEffect : MonoBehaviour
    {
        private void OnEnable()
        {
            Invoke(nameof(DestroyObject), 2f);
        }

        private void DestroyObject() => gameObject.SetActive(false);
        
        private void OnDisable()
        {
            CancelInvoke();
            StackObjectPool.ReturnToPool(gameObject);
        }
    }
}