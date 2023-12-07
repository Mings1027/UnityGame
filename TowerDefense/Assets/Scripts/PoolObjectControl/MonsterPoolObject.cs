using CustomEnumControl;
using UnityEngine;

namespace PoolObjectControl
{
    [DisallowMultipleComponent]
    public class MonsterPoolObject : MonoBehaviour
    {
        public MonsterPoolObjectKey MonsterPoolObjKey { get; set; }

        private void OnDisable()
        {
            PoolObjectManager.ReturnToPool(gameObject, MonsterPoolObjKey);
        }
    }
}