using UnityEngine;

namespace LobbyUIControl
{
    public class LobbyCamController : MonoBehaviour
    {
        [SerializeField] private float rotateSpeed;

        private void Start()
        {
            Time.timeScale = 1;
        }

        private void LateUpdate()
        {
            transform.Rotate(Vector3.up * (rotateSpeed * Time.deltaTime));
        }
    }
}