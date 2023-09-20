using System.Threading;
using Cysharp.Threading.Tasks;
using DataControl;
using GameControl;
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
        private CancellationTokenSource _cts;
        public Vector3 unitSpawnPosition { get; set; }

        private FriendlyUnit[] _units;
        [SerializeField] private PoolObjectKey unitTypeName;
        [SerializeField] private float[] unitHealth;

        /*=========================================================================================================================================
        *                                               Unity Event
        =========================================================================================================================================*/
        protected override void Awake()
        {
            base.Awake();
            _units = new FriendlyUnit[3];
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
            _deadUnitCount = 0;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            _cts?.Cancel();
            if (!_isUnitSpawn) return;

            _isUnitSpawn = false;

            for (var i = 0; i < _units.Length; i++)
            {
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
            _deadUnitCount++;

            if (_deadUnitCount < 3) return;
            _deadUnitCount = 0;
            UnitReSpawnDelay().Forget();
        }

        private async UniTaskVoid UnitReSpawnDelay()
        {
            await UniTask.Delay(5000, cancellationToken: _cts.Token);

            UnitSpawn();
            UnitInit(_damage, _atkDelay);
        }
    }
}