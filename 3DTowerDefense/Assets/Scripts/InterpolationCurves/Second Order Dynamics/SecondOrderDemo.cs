using UnityEngine;

namespace InterpolationCurves.Second_Order_Dynamics
{
    public class SecondOrderDemo : MonoBehaviour
    {
        [Range(0, 10)] public float f;
        [Range(0, 2)] public float z;
        [Range(-10, 10)] public float r;

        [SerializeField] private Transform target;

        private float _f0, _z0, _r0;

        private SecondOrderDynamics _func;

        private void Awake() => InitFunction();

        private void FixedUpdate()
        {
            if (!f.Equals(_f0) || !r.Equals(_r0) || !z.Equals(_z0))
            {
                InitFunction();
            }
            else
            {
                var funcOutput = _func.Update(Time.fixedDeltaTime, target.position);

                if (funcOutput != null)
                {
                    transform.position = new Vector3(funcOutput.Value.x, funcOutput.Value.y, funcOutput.Value.z);
                }
            }
        }

        private void InitFunction()
        {
            _f0 = f;
            _z0 = z;
            _r0 = r;

            _func = new SecondOrderDynamics(f, z, r, transform.position);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, target.position);
        }
    }
}