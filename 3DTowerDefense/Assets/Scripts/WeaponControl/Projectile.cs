using System;
using GameControl;
using UnityEngine;

namespace WeaponControl
{
    public class Projectile : MonoBehaviour
    {
        private Rigidbody _rigid;
        public Vector3 dir;
        private void Awake()
        {
            _rigid = GetComponent<Rigidbody>();
        }

        private void FixedUpdate()
        {
            _rigid.velocity = dir * 7;
        }

        private void OnDisable()
        {
            StackObjectPool.ReturnToPool(gameObject);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Enemy") || other.CompareTag("Ground")) gameObject.SetActive(false);
        }
    }
}