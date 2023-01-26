using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Player
{
    public class PlayerInputController : MonoBehaviour
    {
        [SerializeField] private InputActionReference movement, attack, dash;
        [SerializeField] private UnityEvent<Vector3, bool> onMoveEvent;
        [SerializeField] private UnityEvent onAttackEvent, onDashEvent;

        private void Update()
        {
            if (attack.action.IsPressed())
            {
                onAttackEvent?.Invoke();
            }
            else if (dash.action.triggered)
            {
                onDashEvent?.Invoke();
            }
        }

        private void FixedUpdate()
        {
            onMoveEvent?.Invoke(movement.action.ReadValue<Vector3>().normalized, attack.action.IsPressed());
        }
    }
}