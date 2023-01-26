using System.Collections;
using UnityEngine;
using UnityEngine.Events;

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

    public void Knockback(GameObject target)
    {
        StopAllCoroutines();
        onBegin?.Invoke();
        if (rigid == null) return;
        Vector3 difference = (transform.position - target.transform.position).normalized;
        rigid.AddForce(difference * thrust, ForceMode.Impulse);

        this.Wait(knockbackTime, () =>
        {
            rigid.velocity = Vector3.zero;
            onDone?.Invoke();
        });
    }
}