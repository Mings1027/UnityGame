using System;
using GameControl;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Player
{
    public class PlayerInput : MonoBehaviour
    {
        [SerializeField] private UnityEvent<Vector2> onMovementInput;
        [SerializeField] private UnityEvent onRoll, onAttack, onFlip, onSkill;

        [SerializeField] private InputActionReference movement, roll, attack, skill;

        [SerializeField] private bool pause;

        private void Update()
        {
            if (pause) return;
            if (attack.action.IsPressed()) onAttack?.Invoke();
            else if (skill.action.triggered) onSkill?.Invoke();
            else if (roll.action.triggered) onRoll?.Invoke();
            onFlip?.Invoke();
        }

        private void FixedUpdate()
        {
            onMovementInput?.Invoke(movement.action.ReadValue<Vector2>().normalized);
        }

        public void GamePause()
        {
            pause = !pause;
        }
    }
}