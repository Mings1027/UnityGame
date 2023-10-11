using System;
using CustomEnumControl;
using InterfaceControl;
using ManagerControl;
using UnityEngine;

namespace MapControl
{
    public class ExpandMapButton : MonoBehaviour, IFingerUp
    {
        public event Action<Vector3> OnExpandMapEvent;

        public void Expand()
        {
            OnExpandMapEvent?.Invoke(transform.position);
        }

        private void OnDisable()
        {
            OnExpandMapEvent = null;
        }

        public void FingerUp()
        {
            OnExpandMapEvent?.Invoke(transform.position);
            SoundManager.Instance.PlayBGM(SoundEnum.WaveStart);
        }
    }
}