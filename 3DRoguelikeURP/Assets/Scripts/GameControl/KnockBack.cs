using UnityEngine;
using UnityEngine.Events;
using static DG.Tweening.DOVirtual;
using Vector3 = UnityEngine.Vector3;

namespace GameControl
{
    public class KnockBack : MonoBehaviour
    {
        private Rigidbody rigid;
        public float thrust;
        [SerializeField] private float knockbackTime;
        [SerializeField] private UnityEvent onBegin, onDone;

        private void Awake()
        {
            rigid = GetComponent<Rigidbody>();
        }

        public void Knockback(Transform target)
        {
            onBegin?.Invoke();
            if (rigid == null) return;
            rigid.isKinematic = false;
            var difference = (transform.position - target.position).normalized;
            difference.y = 0;
            rigid.AddForce(difference * thrust, ForceMode.VelocityChange);
            DelayedCall(knockbackTime, () =>
            {
                rigid.velocity = Vector3.zero;
                rigid.isKinematic = true;
                onDone?.Invoke();
            });
        }
    }
}