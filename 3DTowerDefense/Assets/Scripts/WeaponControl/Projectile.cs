using System;
using GameControl;
using UnityEngine;
using UnityEngine.Serialization;

namespace WeaponControl
{
    public class Projectile : MonoBehaviour
    {
        private Rigidbody _rigid;
        public Vector3 dir;
        public Vector3 lookVec;
        private void Awake()
        {
            _rigid = GetComponent<Rigidbody>();
        }

        private void FixedUpdate()
        {
            _rigid.velocity = dir * 7;
            _rigid.transform.LookAt(lookVec);
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