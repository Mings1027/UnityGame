using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class PlayerInput : MonoBehaviour
{
    [SerializeField] private UnityEvent<Vector3> onMovement;
    [SerializeField] private UnityEvent onAttack, onDash;

    [SerializeField] private InputActionReference movement, attack, dash;

    private void Update()
    {
        onMovement?.Invoke(movement.action.ReadValue<Vector3>().normalized);

        if (attack.action.triggered) onAttack?.Invoke();
        else if (dash.action.triggered) onDash?.Invoke();

        // Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Enemy"), true);
    }
}
