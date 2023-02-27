using GameControl;
using UnityEngine;

namespace UnitControl
{
    public abstract class Unit : MonoBehaviour
    {
        protected virtual void OnDisable()
        {
            StackObjectPool.ReturnToPool(gameObject);
        }
    }
}