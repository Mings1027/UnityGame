using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace GameControl
{
    [DisallowMultipleComponent]
    public class PoolObjectController : MonoBehaviour
    {
        [SerializeField] private float lifeTime;
        [SerializeField] private bool dontDestroy;

        private void Awake()
        {
            if (dontDestroy) return;
            if (lifeTime == 0)
                lifeTime = 3;
        }

        private void OnEnable()
        {
            if (dontDestroy) return;
            Invoke(nameof(DestroyObject), lifeTime);
        }

        private void OnDisable()
        {
            if (IsInvoking())
            {
                CancelInvoke();
            }

            ObjectPoolManager.ReturnToPool(gameObject);
        }

        private void DestroyObject()
        {
            gameObject.SetActive(false);
        }
    }
}