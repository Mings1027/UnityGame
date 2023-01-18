using UnityEngine;
using UnityEngine.InputSystem;

namespace PlayerControl
{
    public class PlayerInputController : MonoBehaviour
    {
        public Vector2 Move { get; private set; }
        public Vector2 Look { get; private set; }


        private void OnMove(InputValue value)
        {
            Move = value.Get<Vector2>();
        }

        private void OnLook(InputValue value)
        {
            Look = value.Get<Vector2>();
        }

    }
}