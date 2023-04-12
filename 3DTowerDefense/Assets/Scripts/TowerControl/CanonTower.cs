using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using GameControl;
using UnityEngine;
using WeaponControl;

namespace TowerControl
{
    public class CanonTower : TowerAttacker
    {
        private CancellationTokenSource cts;
        private Sequence atkSequence;
        private Transform[] _singleShootPoints;
        private Transform[] _multiShootPoints;

        [SerializeField] private MeshFilter[] canonMeshFilters;

        protected override void Awake()
        {
            base.Awake();
            var singlePoint = GameObject.Find("SingleShootPoint").transform;
            _singleShootPoints = new Transform[singlePoint.childCount];
            for (var i = 0; i < _singleShootPoints.Length; i++)
            {
                _singleShootPoints[i] = singlePoint.GetChild(i);
            }

            var multiPoint = GameObject.Find("MultiShootPoint").transform;
            _multiShootPoints = new Transform[multiPoint.childCount];
            for (var i = 0; i < _multiShootPoints.Length; i++)
            {
                _multiShootPoints[i] = multiPoint.GetChild(i);
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
            Shoot(endPos, _singleShootPoints[TowerLevel].position + new Vector3(0, 1, 0));
        }

        private async UniTaskVoid MultiShoot(Transform endPos)
        {
            for (var i = 0; i < 3; i++)
            {
                Shoot(endPos, _multiShootPoints[i].position + new Vector3(0, 1, 0));
                await UniTask.Delay(100, cancellationToken: cts.Token);
            }
        }

        private void Shoot(Transform t, Vector3 pos)
        {
            StackObjectPool.Get("CanonSmoke", pos);
            var m = StackObjectPool.Get<Projectile>("CanonBullet", pos);
            m.Init(t, Damage);
            if (m.TryGetComponent(out CanonBullet u))
                u.CanonMeshFilter.sharedMesh = canonMeshFilters[TowerLevel].sharedMesh;
        }
    }
}