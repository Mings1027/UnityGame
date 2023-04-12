using UnityEngine;

namespace GameControl
{
    public class PoolObjectController : MonoBehaviour
    {
        [SerializeField] private bool dontDestroy;
        [SerializeField] private float lifeTime;

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

            StackObjectPool.ReturnToPool(gameObject);
        }

        private void DestroyObject()
        {
            gameObject.SetActive(false);
        }
    }
}