using CustomEnumControl;
using DataControl.TowerDataControl;
using InterfaceControl;
using PoolObjectControl;
using UnityEngine;

namespace ProjectileControl
{
    public abstract class Projectile : MonoBehaviour
    {
        private TrailController _trailObj;

        private Vector3 _startPos;
        private Vector3 _centerPos;

        protected Collider target;
        protected Vector3 curPos;
        protected sbyte effectIndex;
        protected bool isArrived;
        protected int damage;
        protected float lerp;

        [SerializeField] protected TargetingTowerData towerData;
        [SerializeField, Range(0, 50)] private byte height;
        [SerializeField] private float speed;
        [SerializeField] private PoolObjectKey trailPoolObjectKey;

        protected virtual void Awake()
        {
        }

        protected virtual void OnEnable()
        {
            _startPos = transform.position;
            lerp = 0;
        }

        private void OnDisable()
        {
            isArrived = false;
        }

        protected virtual void Update()
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

        protected virtual void ProjectilePath(Vector3 endPos)
        {
            var gravity = Mathf.Lerp(0.8f, 1.5f, lerp);
            lerp += Time.deltaTime * gravity * speed;
            _centerPos = (_startPos + endPos) * 0.5f + Vector3.up * height;
            curPos = Vector3.Lerp(Vector3.Lerp(_startPos, _centerPos, lerp),
                Vector3.Lerp(_centerPos, endPos, lerp), lerp);
        }

        protected void DisableProjectile()
        {
            isArrived = true;
            lerp = 0;
            Hit(target);
            _trailObj.DisconnectProjectile();
            gameObject.SetActive(false);
        }

        public virtual void Init(int dmg, sbyte vfxIndex, Collider t)
        {
            damage = dmg;
            effectIndex = vfxIndex;
            target = t;

            _trailObj = PoolObjectManager.Get<TrailController>(trailPoolObjectKey, _startPos);
            _trailObj.SetProjectileTransform(transform, towerData.projectileColor[vfxIndex]);
        }

        protected virtual void Hit(Collider t)
        {
            if (!t.TryGetComponent(out IDamageable damageable) || !t.enabled) return;
            damageable.Damage(damage);
        }
    }
}