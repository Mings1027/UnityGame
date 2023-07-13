using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DataControl;
using GameControl;
using UnitControl;
using UnitControl.FriendlyControl;
using UnityEngine;

namespace TowerControl
{
    public abstract class UnitTower : Tower
    {
        private int _deadUnitCount;
        private bool _unitIsFull;
        private CancellationTokenSource _cts;

        private Vector3 _unitSpawnPosition;

        [SerializeField] private int unitCount;
        [SerializeField] private FriendlyUnit[] units;
        [SerializeField] private Sprite[] unitSprites;

        protected override void OnEnable()
        {
            base.OnEnable();
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            _cts?.Cancel();
            UnitDisable();
            _unitSpawnPosition = Vector3.zero;
        }

        public override void BuildTowerDelay(MeshFilter consMeshFilter, int minDamage, int maxDamage, float attackRange,
            float attackDelay,
            float health = 0)
        {
            base.BuildTowerDelay(consMeshFilter, minDamage, maxDamage, attackRange, attackDelay, health);

            if (TowerLevel == 0)
            {
                UnitSpawn(minDamage, maxDamage, attackDelay, health);
                var t = transform;
                _unitSpawnPosition = t.position - t.forward * 10;
            }
            else
            {
                UnitUpgrade(minDamage, maxDamage, attackDelay, health);
            }
        }

        public override void BuildTower(MeshFilter towerMeshFilter)
        {
            base.BuildTower(towerMeshFilter);

            if (TowerLevel == 0)
            {
                UnitMove(_unitSpawnPosition);
            }
            else if (IsUniqueTower)
            {
                for (int i = 0; i < unitCount; i++)
                {
                    ChangeUnitSprite(units[i], 1);
                }
            }
        }

        private void UnitSpawn(int minDamage, int maxDamage, float delay, float health)
        {
            for (int i = 0; i < unitCount; i++)
            {
                units[i].transform.position = transform.position;
                units[i].gameObject.SetActive(true);
                units[i].Init(minDamage, maxDamage, delay, health);
                units[i].OnDeadEvent += ReSpawnUnit;
            }

            _unitIsFull = true;
        }

        private void UnitDisable()
        {
            for (int i = 0; i < unitCount; i++)
            {
                units[i].gameObject.SetActive(false);
                ChangeUnitSprite(units[i], 0);
            }

            _unitIsFull = false;
        }

        private void UnitUpgrade(int minDamage, int maxDamage, float delay, float health)
        {
            for (int i = 0; i < unitCount; i++)
            {
                units[i].Init(minDamage, maxDamage, delay, health);
            }
        }

        private void ChangeUnitSprite(FriendlyUnit unit, int unitSpriteIndex)
        {
            var unitSprite = unit.transform.GetChild(0).GetComponent<SpriteRenderer>();
            unitSprite.sprite = unitSprites[unitSpriteIndex];
        }

        public void UnitMove(Vector3 touchPos)
        {
            _unitSpawnPosition = touchPos;
            for (int i = 0; i < unitCount; i++)
            {
                units[i].MoveToTouchPos(_unitSpawnPosition);
            }
        }

        private void ReSpawnUnit(FriendlyUnit u)
        {
            if (isSold) return;
            if (u.GetComponent<Health>().IsDead)
            {
                _deadUnitCount++;
            }

            if (_deadUnitCount < 3) return;
            ReSpawnDelay().Forget();
        }

        private async UniTaskVoid ReSpawnDelay()
        {
            _deadUnitCount = 0;
            _unitIsFull = false;

            await UniTask.Delay(5000, cancellationToken: _cts.Token);

            if (_unitIsFull) return;
            for (int i = 0; i < unitCount; i++)
            {
                units[i].transform.position = transform.position;
                units[i].gameObject.SetActive(true);
            }

            UnitMove(_unitSpawnPosition);
        }
    }
}