using UnityEngine;

namespace InterpolationCurves
{
    public class FirstOrder : MonoBehaviour
    {
        public Transform target;
        public float speed;
        public float stepHeight;
        private Vector3 curPos;

        public void FixedUpdate()
        {
            var oldPos = transform.position;
            curPos = Vector3.Lerp(oldPos, target.position, speed);
            transform.position = curPos;
        }

        // public static Vector3 Lerp(Vector3 start, Vector3 finish, float percentage)
        // {
        //     percentage = Mathf.Clamp01(percentage);
        //     var startToFinish = finish - start;
        //     return start + startToFinish * percentage;
        // }
    }
}