using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
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
        if (target == null) return;
        var position = target.position;
        transform.forward = (position - rigid.position).normalized;
        rigid.DoMove(position + new Vector3(0, 1, 0), bulletSpeed).SetSpeedBased();
    }

    // private void FixedUpdate()
    // {
    //     var dir = (target.position - rigid.position).normalized;
    //     rigid.velocity = dir * bulletSpeed;
    //     transform.forward = dir;
    // }

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