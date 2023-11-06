using CustomEnumControl;
using UnityEngine;

namespace PoolObjectControl
{
    [DisallowMultipleComponent]
    public class EnemyPoolObject : MonoBehaviour
    {
        public EnemyPoolObjectKey enemyPoolObjKey { get; set; }

        private void OnDisable()
        {
            PoolObjectManager.ReturnToPool(gameObject, enemyPoolObjKey);
        }
    }
}