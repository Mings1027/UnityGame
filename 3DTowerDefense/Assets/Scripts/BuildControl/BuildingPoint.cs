using GameControl;
using UnityEngine;

namespace BuildControl
{
    [DisallowMultipleComponent]
    public class BuildingPoint : MonoBehaviour
    {
        private void OnDisable()
        {
            ObjectPoolManager.ReturnToPool(gameObject);
        }
    }
}