using UnityEngine;

namespace GameControl
{
    public class PoolObjectController : MonoBehaviour
    {
        private float lifeTime;
        [SerializeField] private bool dontDestroy;

        private void OnEnable()
        {
            if (dontDestroy) return;
            lifeTime = 3;
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