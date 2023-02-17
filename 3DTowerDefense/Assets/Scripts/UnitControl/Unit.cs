using GameControl;
using UnityEngine;

namespace UnitControl
{
    public class Unit : MonoBehaviour
    {
        private void OnDisable()
        {
            StackObjectPool.ReturnToPool(gameObject);
        }
    }
}
