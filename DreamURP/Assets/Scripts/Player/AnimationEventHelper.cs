using UnityEngine;
using UnityEngine.Events;

namespace Player
{
    public class AnimationEventHelper : MonoBehaviour
    {
        [SerializeField] private UnityEvent onEventTrigger;
        [SerializeField] private UnityEvent onAttackTrigger;

        public void TriggerEvent()
        {
            onEventTrigger?.Invoke();
        }

        public void TriggerAttack()
        {
            onAttackTrigger?.Invoke();
        }
    }
}