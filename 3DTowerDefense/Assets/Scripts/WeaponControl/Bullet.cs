using System;
using System.Collections;
using System.Collections.Generic;
using GameControl;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    private Transform target;
    private int damage;
    private Rigidbody rigid;
    private AudioSource audioSource;

    [SerializeField] private string tagName;
    [SerializeField] private float bulletSpeed;
    [SerializeField] private AudioClip enableAudio;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
    }

    private void OnEnable()
    {
        audioSource.PlayOneShot(enableAudio);
    }

    private void FixedUpdate()
    {
        var dir = (target.position - rigid.position).normalized;
        rigid.velocity = dir * bulletSpeed;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(tagName))
        {
            ProjectileHit(other);
            gameObject.SetActive(false);
        }
    }

    private void ProjectileHit(Component other)
    {
        var pos = transform.position;
        StackObjectPool.Get("BulletExplosionSFX", pos);
        if (other.TryGetComponent(out Health h))
        {
            h.TakeDamage(damage, other.gameObject);
        }
    }

    public void Init(Transform t, int d)
    {
        target = t;
        damage = d;
    }
}