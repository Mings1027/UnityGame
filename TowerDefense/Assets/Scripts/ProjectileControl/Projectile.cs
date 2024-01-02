using DataControl.TowerDataControl;
using DG.Tweening;
using InterfaceControl;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace ProjectileControl
{
    public abstract class Projectile : MonoBehaviour
    {
        private ParticleSystemRenderer _meshParticle;
        private ParticleSystem _trailParticle;

        private DecalProjector _shadowDecal;
        private Tween _destroyTween;

        private Vector3 _curPos;
        private Vector3 _startPos;
        private Vector3 _centerPos;

        protected sbyte effectIndex;
        protected bool isArrived;
        protected int damage;
        protected float lerp;
        protected Collider target;

        [SerializeField] protected TargetingTowerData towerData;
        [SerializeField, Range(0, 50)] private byte height;
        [SerializeField] private float speed;

        protected virtual void Awake()
        {
            _meshParticle = transform.GetChild(0).GetComponent<ParticleSystemRenderer>();
            _trailParticle = transform.GetChild(1).GetComponent<ParticleSystem>();
            _shadowDecal = transform.GetChild(2).GetComponent<DecalProjector>();
            _destroyTween = DOVirtual.DelayedCall(2, () => gameObject.SetActive(false)).SetAutoKill(false).Pause();
        }

        protected virtual void OnEnable()
        {
            _shadowDecal.enabled = true;
            _meshParticle.enabled = true;
            _startPos = transform.position;
            _trailParticle.Play();
            lerp = 0;
        }

        private void OnDisable()
        {
            isArrived = false;
        }

        protected virtual void FixedUpdate()
        {
            if (isArrived) return;
            if (lerp < 1)
            {
                ProjectilePath(target.bounds.center);
            }
            else
            {
                DisableProjectile();
            }
        }

        /*============================================================================================================
         *                                  Unity Event
         ============================================================================================================*/

        protected void ProjectilePath(Vector3 endPos)
        {
            var gravity = Mathf.Lerp(0.8f, 1.5f, lerp);
            lerp += Time.deltaTime * gravity * speed;
            _centerPos = (_startPos + endPos) * 0.5f + Vector3.up * height;
            _curPos = Vector3.Lerp(Vector3.Lerp(_startPos, _centerPos, lerp),
                Vector3.Lerp(_centerPos, endPos, lerp), lerp);
            var t = transform;
            var dir = (_curPos - t.position).normalized;
            if (dir == Vector3.zero) return;
            t.position = _curPos;
            _meshParticle.transform.rotation = Quaternion.LookRotation(dir);
        }

        protected void DisableProjectile()
        {
            isArrived = true;
            lerp = 0;
            _shadowDecal.enabled = false;
            _meshParticle.enabled = false;
            _destroyTween.Restart();
            Hit(target);
        }

        public virtual void Init(int dmg, Collider t)
        {
            damage = dmg;
            target = t;
        }

        public void ColorInit(sbyte vfxIndex)
        {
            effectIndex = vfxIndex;
            var trailColor = _trailParticle.colorOverLifetime;
            trailColor.color = towerData.ProjectileColor[vfxIndex];
        }

        protected virtual void Hit(Collider t)
        {
            if (!t.TryGetComponent(out IDamageable damageable) || !t.enabled) return;
            damageable.Damage(damage);
        }
    }
}