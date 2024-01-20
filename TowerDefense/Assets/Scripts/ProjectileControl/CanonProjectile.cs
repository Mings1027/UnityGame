using System.Diagnostics;
using CustomEnumControl;
using InterfaceControl;
using ManagerControl;
using PoolObjectControl;
using UnityEngine;

namespace ProjectileControl
{
    public class CanonProjectile : Projectile
    {
        private bool _isLockOnTarget;
        private Vector3 _targetEndPos;

        private Collider[] _targetColliders;

        private LayerMask _monsterLayer;

        [SerializeField] private AudioClip audioClip;
        [SerializeField] private byte atkRange;
        [SerializeField] private PoolObjectKey hitPoolObjectKey;

        protected override void Awake()
        {
            base.Awake();
            _monsterLayer = LayerMask.GetMask("Monster") | LayerMask.GetMask("FlyingMonster");
            _targetColliders = new Collider[5];
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            _isLockOnTarget = false;
        }

        protected override void FixedUpdate()
        {
            if (isArrived) return;
            if (lerp < 1)
            {
                if (lerp >= 0.4f && !_isLockOnTarget)
                {
                    _isLockOnTarget = true;
                    _targetEndPos = target.transform.position;
                    _targetEndPos.y = 0;
                }

                ProjectilePath(lerp < 0.4f ? target.transform.position : _targetEndPos);
            }
            else
            {
                DisableProjectile();
            }
        }

        [Conditional("UNITY_EDITOR")]
        private void OnDrawGizmos()
        {
            Gizmos.DrawWireSphere(transform.position, atkRange);
        }

        protected override void Hit(Collider col)
        {
            var mainModule = PoolObjectManager.Get<ParticleSystem>(hitPoolObjectKey, transform.position).main;
            mainModule.startColor = towerData.ProjectileColor[effectIndex];
            SoundManager.Play3DSound(audioClip, transform.position);
            if (_targetEndPos == Vector3.zero) return;
            var pos = transform.position;

            var size = Physics.OverlapSphereNonAlloc(pos, atkRange, _targetColliders, _monsterLayer);

            if (size <= 0) return;
            var dividedDamage = 1.0f / size * damage;
            for (var i = 0; i < size; i++)
            {
                var t = _targetColliders[i];
                if (!t.TryGetComponent(out IDamageable damageable) || !t.enabled) return;
                damageable.Damage(dividedDamage);
            }
        }

        public override void Init(int dmg, Collider t)
        {
            base.Init(dmg, t);
            Physics.Raycast(t.bounds.center + t.transform.forward * 2 + Vector3.up * 2, Vector3.down, out var hit, 10);
            _targetEndPos = hit.point;
            _targetEndPos.y = 0;
        }
    }
}