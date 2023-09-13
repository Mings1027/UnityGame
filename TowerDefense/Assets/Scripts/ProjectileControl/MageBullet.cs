using System;
using DataControl;
using GameControl;
using InterfaceControl;
using ManagerControl;
using UnitControl.EnemyControl;
using UnityEngine;

namespace ProjectileControl
{
    public sealed class MageBullet : MonoBehaviour
    {
        private Transform _target;
        private Rigidbody _rigid;
        private int _damage;

        private DeBuffData.SpeedDeBuffData _speedDeBuffData;

        private Vector3 _startPos;
        private Vector3 _curPos;
        private float _lerp;
        private string _towerName;

        [SerializeField] private float bulletSpeed;

        private void Awake()
        {
            _rigid = GetComponent<Rigidbody>();
            _towerName = TowerType.Mage.ToString();
        }

        private void OnEnable()
        {
            _startPos = transform.position;
        }

        private void FixedUpdate()
        {
            ProjectilePath();
        }

        private void OnDisable()
        {
            _lerp = 0;
            ObjectPoolManager.ReturnToPool(gameObject);
            if (_target == null) return;
            TryHit();
        }

        private void ProjectilePath()
        {
            _lerp += Time.deltaTime * bulletSpeed;
            _curPos = Vector3.Lerp(_startPos, _target.position, _lerp);
            _rigid.position = _curPos;
            if (_lerp >= 1) gameObject.SetActive(false);
        }

        public void Init(Transform target, int damage, DeBuffData.SpeedDeBuffData speedDeBuffData)
        {
            _target = target;
            _damage = damage;

            _speedDeBuffData = speedDeBuffData;
        }

        private void TryHit()
        {
            ObjectPoolManager.Get(StringManager.BloodVfx, transform.position);
            if (_target.TryGetComponent(out IDamageable damageable))
            {
                damageable.Damage(_damage);
                DataManager.SumDamage(_towerName, _damage);
            }

            if (_target.TryGetComponent(out EnemyStatus e))
            {
                e.SlowEffect(_speedDeBuffData).Forget();
            }
        }
    }
}