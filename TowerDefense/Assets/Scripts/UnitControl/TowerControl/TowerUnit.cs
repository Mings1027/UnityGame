using CustomEnumControl;
using Cysharp.Threading.Tasks;
using EPOOutline;
using TowerControl;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UnitControl.TowerControl
{
    public class TowerUnit : Unit, IPointerDownHandler, IPointerUpHandler
    {
        private UnitTower _parentTower;
        private Vector3 _originPos;

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
            isAttacking = false;
            Move(_originPos).Forget();
            unitState = UnitState.Patrol;
            target = null;
            if (_moveInput) return;
            anim.SetBool(IsWalk, false);
        }

        public async UniTaskVoid Move(Vector3 pos)
        {
            _originPos = pos;
            _moveInput = true;
            anim.SetBool(IsWalk, true);
            navMeshAgent.stoppingDistance = 0.1f;
            navMeshAgent.SetDestination(pos);
            while (gameObject.activeSelf && Vector3.Distance(transform.position, navMeshAgent.destination) >
                   navMeshAgent.stoppingDistance)
            {
                await UniTask.Delay(100);
            }

            _moveInput = false;
            navMeshAgent.stoppingDistance = 1;
            anim.SetBool(IsWalk, false);
        }

        public void InfoInit(UnitTower unitTower, Vector3 pos)
        {
            _originPos = pos;
            _parentTower = unitTower;
        }

        public void DisableParent()
        {
            _parentTower = null;
        }

        public void UnitUpgrade(int damage, int healthAmount, float attackDelayData)
        {
            this.damage = damage;
            health.Init(healthAmount);
            atkDelay = attackDelayData;
        }
    }
}