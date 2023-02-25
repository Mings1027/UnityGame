using System;
using GameControl;
using UnityEngine;

namespace UnitControl
{
    public class BarracksUnit : MonoBehaviour
    {
        public event Action OnDeadEvent;

        private void OnDisable()
        {
            OnDeadEvent?.Invoke();
            StackObjectPool.ReturnToPool(gameObject);
        }
        
        
    }
}
