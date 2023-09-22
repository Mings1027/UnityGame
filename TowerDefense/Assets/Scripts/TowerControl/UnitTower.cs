using Cysharp.Threading.Tasks;
using DG.Tweening;
using PoolObjectControl;
using UnitControl.FriendlyControl;
using UnityEngine;

namespace TowerControl
{
    public class UnitTower : Tower
    {
        private Collider[] _targetColliders;
        private bool _isUnitSpawn;
        private int _deadUnitCount;
        private int _damage;
        private float _atkDelay;
        private bool _isSell;

        public Vector3 unitSpawnPosition { get; set; }

        private FriendlyUnit[] _units;
        [SerializeField] private PoolObjectKey unitTypeName;
        [SerializeField] private float[] unitHealth;

        /*=========================================================================================================================================
        *                                               Unity Event
        =========================================================================================================================================*/
        protected override void Init()
        {
            base.Init();
            _units = new FriendlyUnit[3];
            _deadUnitCount = 0;
            _isSell = false;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            if (_isUnitSpawn)
            {
                for (int i = 0; i < _units.Length; i++)
                {
                    _units[i].StartTargeting(true);
                }
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            if (_isUnitSpawn)
            {
                for (int i = 0; i < _units.Length; i++)
                {
                    _units[i].StartTargeting(false);
                }
            }
        }

        private void OnDestroy()
        {
            _isSell = true;
            if (!_isUnitSpawn) return;

            _isUnitSpawn = false;

            for (var i = 0; i < _units.Length; i++)
            {
                if (_units[i] == null) continue;
                _units[i].gameObject.SetActive(false);
                _units[i] = null;
            }
        }

        /*=========================================================================================================================================
        *                                               Unity Event
        =========================================================================================================================================*/
        public override void FingerUp()
        {
            base.FingerUp();
            ActiveUnitIndicator();
        }

        private void ActiveUnitIndicator()
        {
            for (var i = 0; i < _units.Length; i++)
            {
                _units[i].Indicator.enabled = true;
            }
        }

        public override void TowerSetting(MeshFilter towerMesh, int damageData, int rangeData,
            float attackDelayData)
        {
            base.TowerSetting(towerMesh, damageData, rangeData, attackDelayData);

            _damage = damageData;
            _atkDelay = attackDelayData;
            if (!_isUnitSpawn)
            {
                _isUnitSpawn = true;
                UnitSpawn();
            }

            UnitInit(damageData, attackDelayData);

            // if (TowerManager.Instance.StartWave) return;
            // for (var i = 0; i < _units.Length; i++)
            // {
            //     _units[i].StartTargeting(false);
            // }
        }

        private void UnitSpawn()
        {
            for (var i = 0; i < _units.Length; i++)
            {
                _units[i] = null;
                var angle = Mathf.PI * 0.5f - i * (Mathf.PI * 2f) / _units.Length;
                var pos = unitSpawnPosition + new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle));
                _units[i] = PoolObjectManager.Get<FriendlyUnit>(unitTypeName, pos);
                PoolObjectManager.Get(PoolObjectKey.UnitSpawnSmoke, pos);
                _units[i].OnReSpawnEvent += UnitReSpawn;
                // _units[i].StartTargeting(true);
            }
        }

        private void UnitInit(int damage, float delay)
        {
            var health = unitHealth[TowerLevel];

            for (var i = 0; i < _units.Length; i++)
            {
                _units[i].Init(this, TowerType, damage, delay, health);
            }
        }

        public async UniTask StartUnitMove(Vector3 touchPos)
        {
            var tasks = new UniTask[_units.Length - _deadUnitCount];
            for (var i = 0; i < tasks.Length; i++)
            {
                var angle = Mathf.PI * 0.5f - i * (Mathf.PI * 2f) / _units.Length;
                var pos = touchPos + new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle));

                tasks[i] = _units[i].MoveToTouchPos(pos);
            }

            await UniTask.WhenAll(tasks);
        }

        public void OffUnitIndicator()
        {
            if (!_isUnitSpawn) return;
            for (int i = 0; i < _units.Length; i++)
            {
                _units[i].Indicator.enabled = false;
            }
        }

        private void UnitReSpawn(FriendlyUnit u)
        {
            if (_isSell) return;
            _deadUnitCount++;

            if (_deadUnitCount < 3) return;
            _deadUnitCount = 0;
            UnitReSpawnDelay();
        }

        private void UnitReSpawnDelay()
        {
            DOVirtual.DelayedCall(5, () =>
            {
                UnitSpawn();
                UnitInit(_damage, _atkDelay);
            }, false);
        }
    }
}