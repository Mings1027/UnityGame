using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DataControl;
using GameControl;
using TowerControl;
using UnitControl.FriendlyControl;
using UnityEngine;

public class UnitTowerTest : Tower
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
    }

    public override void BuildTowerDelay(MeshFilter consMeshFilter, int minDamage, int maxDamage, float attackRange,
        float attackDelay,
        float health = 0)
    {
        base.BuildTowerDelay(consMeshFilter, minDamage, maxDamage, attackRange, attackDelay, health);

        if (TowerLevel == 0)
        {
            UnitSpawn(minDamage, maxDamage, attackDelay, health);
        }
        else
        {
            UnitUpgrade(minDamage, maxDamage, attackDelay, health);
        }
    }

    public override void BuildTower(MeshFilter towerMeshFilter)
    {
        base.BuildTower(towerMeshFilter);
        var t = transform;
        UnitMove(t.position - t.forward * 10);
    }

    private void UnitSpawn(int minDamage, int maxDamage, float delay, float health)
    {
        for (int i = 0; i < unitCount; i++)
        {
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
            var unitSprite = units[i].transform.GetChild(0).GetComponent<SpriteRenderer>();
            unitSprite.sprite = unitSprites[0];
        }

        _unitIsFull = false;
    }

    private void UnitUpgrade(int minDamage, int maxDamage, float delay, float health)
    {
        for (int i = 0; i < unitCount; i++)
        {
            units[i].Init(minDamage, maxDamage, delay, health);
        }

        if (IsUniqueTower)
        {
            for (int i = 0; i < unitCount; i++)
            {
                var unitSprite = units[i].transform.GetChild(0).GetComponent<SpriteRenderer>();
                unitSprite.sprite = unitSprites[1];
            }
        }
    }

    public void UnitMove(Vector3 touchPos)
    {
        for (int i = 0; i < unitCount; i++)
        {
            units[i].MoveToTouchPos(touchPos);
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
            units[i].gameObject.SetActive(true);
        }

        var t = transform;
        UnitMove(t.position - t.forward * 10);
    }
}