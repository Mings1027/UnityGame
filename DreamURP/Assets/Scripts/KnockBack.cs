using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class KnockBack : MonoBehaviour
{
    private Rigidbody2D rigid;
    [HideInInspector] public float thrust;
    [SerializeField] private float knockbackTime;
    [SerializeField] private float canAttackTime;
    [SerializeField] private UnityEvent onBegin, onDone;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
    }

    public void Knockback(GameObject target)
    {
        StopAllCoroutines();
        onBegin?.Invoke();
        if (rigid == null) return;
        Vector2 difference = (transform.position - target.transform.position).normalized;
        rigid.AddForce(difference * thrust, ForceMode2D.Impulse);

        this.Wait(knockbackTime, () =>
        {
            rigid.velocity = Vector2.zero;
            this.Wait(canAttackTime, () => onDone?.Invoke());
        });
    }
}