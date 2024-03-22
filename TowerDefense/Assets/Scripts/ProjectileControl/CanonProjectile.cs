using System.Diagnostics;
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

        [SerializeField] private LayerMask monsterLayer;
        [SerializeField] private AudioClip audioClip;
        [SerializeField] private byte atkRange;

        private void Awake()
        {
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
            if (lerpTime < 1)
            {
                if (lerpTime >= 0.4f && !_isLockOnTarget)
                {
                    _isLockOnTarget = true;
                    _targetEndPos = target.transform.position;
                    _targetEndPos.y = 0;
                }

                ProjectilePath(lerpTime < 0.4f ? target.transform.position : _targetEndPos);
            }
            else
            {
                DisableObject();
            }
        }

        [Conditional("UNITY_EDITOR")]
        private void OnDrawGizmos()
        {
            Gizmos.DrawWireSphere(transform.position, atkRange);
        }

        protected override void ProjectilePath(Vector3 endPos)
        {
            base.ProjectilePath(endPos);
            var dir = (curPos - transform.position).normalized;
            if (dir == Vector3.zero) return;
            transform.SetPositionAndRotation(curPos, Quaternion.LookRotation(dir));
        }

        protected override void Hit(Collider col)
        {
            var position = transform.position;
            SoundManager.Play3DSound(audioClip, position);

            var mainModule = PoolObjectManager.Get<ParticleSystem>(hitParticleKey, position).main;
            mainModule.startColor = towerData.projectileColor[effectIndex];

            if (_targetEndPos == Vector3.zero) return;

            var size = Physics.OverlapSphereNonAlloc(position, atkRange, _targetColliders, monsterLayer);

            if (size <= 0) return;
            var dividedDamage = (int)(1.0f / size * damage);
            for (var i = 0; i < size; i++)
            {
                var t = _targetColliders[i];
                if (!t.TryGetComponent(out IDamageable damageable) || !t.enabled) return;
                damageable.Damage(dividedDamage);
            }
        }

        public override void Init(int dmg, sbyte vfxIndex, Collider t)
        {
            base.Init(dmg, vfxIndex, t);
            Physics.Raycast(t.bounds.center + t.transform.forward * 2 + Vector3.up * 2, Vector3.down, out var hit, 10);
            _targetEndPos = hit.point;
            _targetEndPos.y = 0;
        }
    }
}