using System;
using UnityEngine;
using UnityEngine.Events;

namespace Enemy
{
    public class OnTriggerEventHelper : MonoBehaviour
    {
        [SerializeField] private string targetTag = "Player";
        [SerializeField] private UnityEvent onTriggerEnterEvent, onTriggerExitEvent;
        
        private void OnTriggerEnter2D(Collider2D col)
        {
            if (col.CompareTag(targetTag))
            {
                onTriggerEnterEvent?.Invoke();
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag(targetTag))
            {
                onTriggerExitEvent?.Invoke();
            }
        }
    }
}