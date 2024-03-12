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
        protected float lerpTime;

        [SerializeField] protected TargetingTowerData towerData;
        [SerializeField, Range(0, 50)] private byte height;
        [SerializeField] private float speed;
        [SerializeField] private PoolObjectKey trailPoolObjectKey;
        [SerializeField] protected PoolObjectKey hitParticleKey;

        protected virtual void Awake()
        {
        }

        protected virtual void OnEnable()
        {
            _startPos = transform.position;
            lerpTime = 0;
        }

        private void OnDisable()
        {
            isArrived = false;
        }

        protected virtual void Update()
        {
            if (isArrived) return;
            if (lerpTime < 1)
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
            var gravity = Mathf.Lerp(0.8f, 1.5f, lerpTime);
            lerpTime += Time.deltaTime * gravity * speed;
            _centerPos = (_startPos + endPos) * 0.5f + Vector3.up * height;
            curPos = Vector3.Lerp(Vector3.Lerp(_startPos, _centerPos, lerpTime),
                Vector3.Lerp(_centerPos, endPos, lerpTime), lerpTime);
        }

        protected void DisableProjectile()
        {
            isArrived = true;
            lerpTime = 0;
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
            _trailObj.SpawnProjectile(transform, towerData.projectileColor[vfxIndex]);
        }

        protected abstract void Hit(Collider t);
    }
}