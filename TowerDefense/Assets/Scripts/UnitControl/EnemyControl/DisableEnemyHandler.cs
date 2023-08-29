using System;
using UnityEngine;

namespace UnitControl.EnemyControl
{
    public class DisableEnemyHandler : MonoBehaviour
    {
        public event Action OnUpdateEnemyCount;
        public event Action OnDecreaseLifeCount;

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Destination")) return;
            
            OnDecreaseLifeCount?.Invoke();
            gameObject.SetActive(false);
        }

        private void OnDisable()
        {
            OnUpdateEnemyCount?.Invoke();
            OnUpdateEnemyCount = null;
            OnDecreaseLifeCount = null;
        }
    }
}