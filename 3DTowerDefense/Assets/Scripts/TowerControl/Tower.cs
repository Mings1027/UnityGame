using AttackControl;
using GameControl;
using UnityEngine;

namespace TowerControl
{
    public abstract class Tower : MonoBehaviour
    {
        // private Outline _outline;
        // private MeshFilter _meshFilter;
        // private bool _isUpgrading;
        // protected bool isSold;        
        // protected CancellationTokenSource cts;        

        private TargetFinder _targetFinder;
        private bool _attackAble;
        private bool _isTargeting;
        
        private float _atkDelay;

        protected int damage;
        protected Transform target;

//==================================Event function=====================================================

        protected virtual void Awake()
        {
            _targetFinder = GetComponent<TargetFinder>();
        }

        protected virtual void OnEnable()
        {
            _attackAble = true;
        }

        protected virtual void OnDisable()
        {
            StackObjectPool.ReturnToPool(gameObject);
            CancelInvoke();
        }

        // protected virtual void Update()
        // {
        //     if (!_attackAble || !_isTargeting) return;
        //     UnitControl();
        //     StartCoolDown().Forget();
        // }


        protected virtual void OnDrawGizmos()
        {
            
        }

        //==================================Event function=====================================================

        //==================================Custom function====================================================



        public virtual void EndBuild(float attackDelay, int unitDamage, int unitHealth)
        {
            _atkDelay = attackDelay;
            damage = unitDamage;
        }

        protected abstract void UnitControl();

        // private async UniTaskVoid StartCoolDown()
        // {
        //     _attackAble = false;
        //     await UniTask.Delay(TimeSpan.FromSeconds(_atkDelay), cancellationToken: cts.Token);
        //     _attackAble = true;
        // }

     
    }
}