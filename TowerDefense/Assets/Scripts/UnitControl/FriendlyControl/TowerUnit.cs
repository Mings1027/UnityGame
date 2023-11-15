using CustomEnumControl;
using Cysharp.Threading.Tasks;
using EPOOutline;
using ManagerControl;
using TowerControl;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UnitControl.FriendlyControl
{
    public class TowerUnit : Unit, IPointerDownHandler, IPointerUpHandler
    {
        private UnitTower _parentTower;
        private Vector3 _originPos;
        // private TowerType _towerTypeEnum;

        private bool _moveInput;

        public Outlinable outline { get; private set; }

        #region Unity Event

        protected override void Awake()
        {
            base.Awake();
            outline = GetComponent<Outlinable>();
            outline.enabled = false;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (Input.touchCount > 1) return;
            if (!Input.GetTouch(0).deltaPosition.Equals(Vector2.zero)) return;
            _parentTower.OnPointerUp(null);
        }

        #endregion

        public void UnitTargetInit()
        {
            Move(_originPos).Forget();
            unitState = UnitState.Patrol;
            target = null;
            if (_moveInput) return;
            anim.SetBool(IsWalk, false);
        }

        public async UniTask Move(Vector3 pos)
        {
            _originPos = pos;
            _moveInput = true;
            anim.SetBool(IsWalk, true);
            navMeshAgent.stoppingDistance = 0.1f;
            navMeshAgent.SetDestination(pos);
            while (Vector3.Distance(transform.position, navMeshAgent.destination) > navMeshAgent.stoppingDistance)
            {
                await UniTask.Yield();
            }

            _moveInput = false;
            navMeshAgent.stoppingDistance = 1;
            anim.SetBool(IsWalk, false);
        }

        public void InfoInit(UnitTower unitTower, /*TowerType towerTypeEnum,*/ Vector3 pos)
        {
            _originPos = pos;
            _parentTower = unitTower;
            // _towerTypeEnum = towerTypeEnum;
        }

        public void UnitUpgrade(int damage, int healthAmount, float attackDelayData)
        {
            this.damage = damage;
            health.Init(healthAmount);
            atkDelay = attackDelayData;
        }
    }
}