using System;
using InterfaceControl;
using UnityEngine;

namespace MapControl
{
    public class ExpandMapButton : MonoBehaviour, IOpenUI
    {
        public event Action<Vector3> OnExpandMapEvent;

        // public void Expand()
        // {
        //     OnExpandMapEvent?.Invoke(transform.position);
        // }

        private void OnDisable()
        {
            OnExpandMapEvent = null;
        }

        public void OpenUI()
        {
            OnExpandMapEvent?.Invoke(transform.position);
        }
    }
}