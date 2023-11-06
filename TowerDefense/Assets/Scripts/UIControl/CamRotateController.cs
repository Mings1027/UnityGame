using UnityEngine;

namespace UIControl
{
    public class CamRotateController : MonoBehaviour
    {
        [SerializeField] private int rotateSpeed;
        
        private void Start()
        {
            Time.timeScale = 1;
        }

        private void Update()
        {
            var rotAmount = Time.deltaTime * rotateSpeed;
            transform.Rotate(Vector3.up, rotAmount);
        }
    }
}