using Cysharp.Threading.Tasks;
using DataControl;
using GameControl;
using UnityEngine;
using WeaponControl;

namespace TowerControl
{
    public class ArcherTargetingTower : TargetingTower
    {
        private int _archerCount;
        private Transform _targetPos;
        private GameObject[] _archerUnits;

        [SerializeField] private Transform[] archerPos;
        [SerializeField] private float smoothTurnSpeed;


        protected override void OnEnable()
        {
            base.OnEnable();
            onAttackEvent += () => ProjectileAttack().Forget();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            UnitDisable();
        }

        private void LateUpdate()
        {
            if (!isTargeting) return;
            var targetPos = target.position + target.forward;

            for (var i = 0; i < _archerCount; i++)
            {
                var dir = targetPos - transform.position;
                var yRot = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
                var lookRot = Quaternion.Euler(0, yRot, 0);
                _archerUnits[i].transform.rotation =
                    Quaternion.Lerp(_archerUnits[i].transform.rotation, lookRot, smoothTurnSpeed);
            }
        }

        protected override void Init()
        {
            base.Init();
            targetColliders = new Collider[5];
            _archerUnits = new GameObject[2];
        }

        public override void TowerInit(MeshFilter consMeshFilter, int minDamage, int maxDamage, float attackRange,
            float attackDelay, float health = 0)
        {
            base.TowerInit(consMeshFilter, minDamage, maxDamage, attackRange, attackDelay, health);

            BatchArcher();
        }

        public override void TowerSetting(MeshFilter towerMeshFilter)
        {
            base.TowerSetting(towerMeshFilter);

            for (var i = 0; i < _archerCount; i++)
            {
                _archerUnits[i].transform.GetChild(0).gameObject.SetActive(true);
            }

            if (!IsUniqueTower || TowerUniqueLevel == 1) return;

            onAttackEvent = null;
            onAttackEvent += BulletAttack;
        }

        private void BatchArcher()
        {
            if (_archerUnits[0] != null) _archerUnits[0].gameObject.SetActive(false);

            _archerCount = TowerUniqueLevel == 1 ? 2 : 1;
            for (var i = 0; i < _archerCount; i++)
            {
                var index = IsUniqueTower ? TowerUniqueLevel + 3 + i : TowerLevel;
                _archerUnits[i] = ObjectPoolManager.Get(PoolObjectName.ArcherUnit, archerPos[index]);
                _archerUnits[i].transform.GetChild(0).gameObject.SetActive(false);
            }
        }

        private void UnitDisable()
        {
            for (var i = 0; i < _archerUnits.Length; i++)
            {
                if (_archerUnits[i] == null || !_archerUnits[i].gameObject.activeSelf) continue;
                _archerUnits[i].gameObject.SetActive(false);
                _archerUnits[i] = null;
            }
        }

        private async UniTaskVoid ProjectileAttack()
        {
            for (var i = 0; i < _archerCount; i++)
            {
                ObjectPoolManager.Get(PoolObjectName.ArrowShootSfx, transform);
                ObjectPoolManager
                    .Get<ArcherProjectile>(PoolObjectName.ArcherProjectile, _archerUnits[i].transform.position)
                    .Init(target, Damage);
                if (!target.gameObject.activeSelf) break;
                await UniTask.Delay(500);
            }
        }

        private void BulletAttack()
        {
            ObjectPoolManager.Get(PoolObjectName.BulletShootSfx, transform);
            ObjectPoolManager.Get<Bullet>(PoolObjectName.ArcherBullet, _archerUnits[0].transform.position)
                .Init(target, Damage);
        }
    }
}