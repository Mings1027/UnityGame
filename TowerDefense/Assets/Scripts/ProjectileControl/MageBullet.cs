using DataControl;
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

        private SphereCollider _sphereCollider;
        private DeBuffData.SpeedDeBuffData _speedDeBuffData;

        private float _timeSinceLastUpdate;
        private bool _isArrived;
        private string _towerName;

        [SerializeField] private float bulletSpeed;
        [SerializeField] private float updateInterval;

        private void Awake()
        {
            _rigid = GetComponent<Rigidbody>();
            _sphereCollider = GetComponent<SphereCollider>();
            _towerName = TowerType.Mage.ToString();
        }

        private void OnEnable()
        {
            _sphereCollider.enabled = true;
        }

        private void FixedUpdate()
        {
            if (_isArrived) return;
            ProjectilePath();
        }

        private void OnDisable()
        {
            _isArrived = false;
        }

        private void OnTriggerEnter(Collider other)
        {
            _isArrived = true;
            _sphereCollider.enabled = false;

            WhenHitEnemy(other);
        }

        private void ProjectilePath()
        {
            _timeSinceLastUpdate += Time.deltaTime;
            if (_timeSinceLastUpdate < updateInterval) return;

            var rigidPos = _rigid.position;
            var dir = (_target.position - rigidPos).normalized;
            _rigid.MovePosition(rigidPos + dir * (bulletSpeed * Time.deltaTime));
            _timeSinceLastUpdate = 0;
        }

        public void Init(Transform target, int damage, DeBuffData.SpeedDeBuffData speedDeBuffData)
        {
            _target = target;
            _damage = damage;

            _speedDeBuffData = speedDeBuffData;
        }

        private void WhenHitEnemy(Component other)
        {
            if (other.TryGetComponent(out IDamageable damageable))
            {
                damageable.Damage(_damage);
                DataManager.SumDamage(_towerName, _damage);
            }

            if (other.TryGetComponent(out EnemyStatus e))
            {
                e.SlowEffect(_speedDeBuffData).Forget();
            }
        }
    }
}