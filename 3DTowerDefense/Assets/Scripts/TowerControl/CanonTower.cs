using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using GameControl;
using UnityEngine;
using WeaponControl;
using Random = UnityEngine.Random;

namespace TowerControl
{
    public class CanonTower : TowerAttacker
    {
        private CancellationTokenSource cts;
        private Sequence atkSequence;
        private Vector3 shootPos;

        private Transform[] _singleShootPoints;
        private Transform[] _multiShootPoints;

        [SerializeField] private MeshFilter[] canonMeshFilters;
        [SerializeField] private int min, max;

        protected override void Awake()
        {
            base.Awake();
            _singleShootPoints = new Transform[transform.GetChild(1).childCount];
            for (var i = 0; i < _singleShootPoints.Length; i++)
            {
                _singleShootPoints[i] = transform.GetChild(1).GetChild(i);
            }

            _multiShootPoints = new Transform[transform.GetChild(2).childCount];
            for (var i = 0; i < _multiShootPoints.Length; i++)
            {
                _multiShootPoints[i] = transform.GetChild(2).GetChild(i);
            }

            atkSequence = DOTween.Sequence().SetAutoKill(false).Pause()
                .Append(meshFilter.transform.DOScaleY(0.5f, 0.5f).SetEase(Ease.OutQuint))
                .AppendCallback(Attack)
                .Append(meshFilter.transform.DOScaleY(1f, 0.5f).SetEase(Ease.OutQuint));
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            cts?.Dispose();
            cts = new CancellationTokenSource();
        }

        protected override void Update()
        {
            if (gameManager.IsPause) return;
            if (isUpgrading || !attackAble || !isTargeting) return;
            atkSequence.Restart();
            StartCoolDown();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            cts?.Cancel();
        }

        private void OnDestroy()
        {
            atkSequence.Kill();
        }

        public override void UnderConstruction(MeshFilter consMeshFilter)
        {
            base.UnderConstruction(consMeshFilter);
            if (TowerLevel <= 3)
                shootPos = _singleShootPoints[TowerLevel].position + new Vector3(0, 1, 0);
        }

        protected override void Attack()
        {
            StackObjectPool.Get("CanonShootSFX", transform.position);
            if (TowerLevel != 4)
            {
                SingleShoot(target);
            }
            else
            {
                MultiShoot(target).Forget();
            }
        }

        private void SingleShoot(Transform endPos)
        {
            StackObjectPool.Get("CanonSmoke", shootPos);
            var m = StackObjectPool.Get<Projectile>("CanonBullet", shootPos);
            m.Init(endPos, Damage);
            if (m.TryGetComponent(out CanonBullet u)) u.ChangeMesh(canonMeshFilters[TowerLevel]);
        }

        private async UniTaskVoid MultiShoot(Transform endPos)
        {
            for (var i = 0; i < 3; i++)
            {
                StackObjectPool.Get("CanonSmoke", _multiShootPoints[i].position + new Vector3(0, 1, 0));
                var m = StackObjectPool.Get<Projectile>("CanonBullet", _multiShootPoints[i].position);
                m.Init(endPos, Damage);
                if (m.TryGetComponent(out CanonBullet u)) u.ChangeMesh(canonMeshFilters[TowerLevel]);

                await UniTask.Delay(100, cancellationToken: cts.Token);
            }
        }
    }
}