using UnityEngine;

namespace TestControl
{
    public class Test : MonoBehaviour
    {
        private void Start()
        {
            Time.timeScale = 0.2f;
        }

        private void FixedUpdate()
        {
            print("fix");
        }

        private void Update()
        {
            print("up");
        }

        private void LateUpdate()
        {
            print("late");
        }
    }
}