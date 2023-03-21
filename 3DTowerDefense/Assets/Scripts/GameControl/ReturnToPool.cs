using UnityEngine;

namespace GameControl
{
    public class ReturnToPool : MonoBehaviour
    {
        private void OnDisable()
        {
            StackObjectPool.ReturnToPool(gameObject);
        }
    }
}
