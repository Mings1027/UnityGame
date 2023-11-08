using CustomEnumControl;
using UnityEngine;

namespace PoolObjectControl
{
    public class UIPoolObject : MonoBehaviour
    {
        public UIPoolObjectKey UIPoolObjKey { get; set; }

        private void OnDisable()
        {
            PoolObjectManager.ReturnToPool(gameObject, UIPoolObjKey);
        }
    }
}