using System;
using InterfaceControl;
using UnityEngine;

namespace UnitControl
{
    [DisallowMultipleComponent]
    public class AttackPoint : MonoBehaviour
    {
        public float damage { get; set; }
        public Transform target { get; set; }

        private void OnEnable()
        {
            if (target.TryGetComponent(out IDamageable damageable))
            {
                damageable.Damage(damage);
            }
        }
    }
}