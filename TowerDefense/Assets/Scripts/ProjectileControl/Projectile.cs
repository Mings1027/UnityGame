using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DataControl;
using DG.Tweening;
using InterfaceControl;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace ProjectileControl
{
    public abstract class Projectile : MonoBehaviour, IHit
    {
        private ParticleSystem _trailParticle;
        private ParticleSystem _hitParticle;
        private MeshRenderer _projectileMesh;
        private DecalProjector _shadowDecal;
        private Tween _destroyTween;

        private float _gravity;
        private Vector3 _curPos;
        private Vector3 _startPos;
        private Vector3 _centerPos;

        protected CancellationTokenSource cts;
        protected int damage;
        protected float lerp;
        protected Collider target;

        [SerializeField] private TargetingTowerData towerData;
        [SerializeField, Range(0, 50)] private float height;
        [SerializeField] private float speed;

        protected virtual void Awake()
        {
            _projectileMesh = transform.GetChild(0).GetComponent<MeshRenderer>();
            _trailParticle = transform.GetChild(1).GetComponent<ParticleSystem>();
            _hitParticle = transform.GetChild(2).GetComponent<ParticleSystem>();
            _shadowDecal = transform.GetChild(3).GetComponent<DecalProjector>();
            _destroyTween = DOVirtual.DelayedCall(2, () => gameObject.SetActive(false)).SetAutoKill(false).Pause();
        }

        protected virtual void OnEnable()
        {
            _shadowDecal.enabled = true;
            _projectileMesh.enabled = true;
            _startPos = transform.position;
            _trailParticle.Play();
            lerp = 0;
            cts?.Dispose();
            cts = new CancellationTokenSource();
        }

        private void OnDisable()
        {
            cts.Cancel();
        }

        private void OnDestroy()
        {
            cts.Dispose();
        }
        /*============================================================================================================
         *                                  Unity Event
         ============================================================================================================*/

        public virtual async UniTaskVoid ProjectileUpdate()
        {
            while (lerp < 1)
            {
                await UniTask.Delay(10, cancellationToken: cts.Token);

                ProjectilePath(target.bounds.center);
            }

            DisableProjectile();
        }

        protected void ProjectilePath(Vector3 endPos)
        {
            _gravity = Mathf.Lerp(0.8f, 1.5f, lerp);
            lerp += Time.deltaTime * _gravity * speed;
            _centerPos = (_startPos + endPos) * 0.5f + Vector3.up * height;
            _curPos = Vector3.Lerp(Vector3.Lerp(_startPos, _centerPos, lerp),
                Vector3.Lerp(_centerPos, endPos, lerp), lerp);
            var t = transform;
            var dir = (_curPos - t.position).normalized;
            if (dir == Vector3.zero) return;
            t.position = _curPos;
            _projectileMesh.transform.rotation = Quaternion.LookRotation(dir);
        }

        protected void DisableProjectile()
        {
            Hit();
            _projectileMesh.enabled = false;
            _shadowDecal.enabled = false;
            _destroyTween.Restart();
        }

        public virtual void Init(int dmg, Collider t)
        {
            damage = dmg;
            target = t;
        }

        public virtual void ColorInit(sbyte effectIndex)
        {
            var trailColor = _trailParticle.main;
            trailColor.startColor = towerData.ProjectileColor[effectIndex];
            var hitColor = _hitParticle.main;
            hitColor.startColor = towerData.ProjectileColor[effectIndex];
        }

        public virtual void Hit()
        {
            _hitParticle.Play();
        }

        protected void TryDamage(Collider t)
        {
            if (!t.TryGetComponent(out IDamageable damageable) || !t.enabled) return;
            damageable.Damage(damage);
        }
    }
}