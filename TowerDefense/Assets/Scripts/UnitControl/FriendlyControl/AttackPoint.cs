using InterfaceControl;
using UnityEngine;

namespace UnitControl.FriendlyControl
{
    public class AttackPoint : MonoBehaviour
    {
        private float _lerp;
        private int _damage;

        private Transform _target;

        private void Update()
        {
            _lerp += Time.deltaTime * 10;
            if (_lerp >= 1) enabled = false;
        }

        private void OnDisable()
        {
            _lerp = 0;
            TryHit();
        }

        private void TryHit()
        {
            if (_target == null) return;
            if (_target.TryGetComponent(out IDamageable damageable))
            {
                damageable.Damage(_damage);
            }
        }

        public void Init(Transform target, int damage)
        {
            _target = target;
            _damage = damage;
        }
    }
}