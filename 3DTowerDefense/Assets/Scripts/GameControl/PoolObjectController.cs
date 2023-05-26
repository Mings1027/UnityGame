using UnityEngine;

namespace GameControl
{
    public class PoolObjectController : MonoBehaviour
    {
        private float _lifeTime;
        [SerializeField] private bool dontDestroy;

        private void OnEnable()
        {
            if (dontDestroy) return;
            _lifeTime = 3;
            Invoke(nameof(DestroyObject), _lifeTime);
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