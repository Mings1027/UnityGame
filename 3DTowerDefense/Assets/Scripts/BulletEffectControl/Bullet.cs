using System;
using GameControl;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    private Rigidbody rigid;

    [SerializeField] private float maxLength;
    [SerializeField] private float bulletSpeed;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        Invoke(nameof(DestroyBullet), 3);
    }

    private void OnDisable()
    {
        CancelInvoke();
        StackObjectPool.ReturnToPool(gameObject);
    }

    private void FixedUpdate()
    {
        rigid.velocity = Vector3.ClampMagnitude(transform.forward, maxLength) * bulletSpeed;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            DestroyBullet();
        }
    }

    private void DestroyBullet()
    {
        gameObject.SetActive(false);
    }
}