using CustomEnumControl;
using UnityEngine;

namespace PoolObjectControl
{
    [DisallowMultipleComponent]
    public class PoolObject : MonoBehaviour
    {
        public PoolObjectKey PoolObjKey { get; set; }

        private void OnDisable()
        {
            PoolObjectManager.ReturnToPool(gameObject, PoolObjKey);
        }
    }
}