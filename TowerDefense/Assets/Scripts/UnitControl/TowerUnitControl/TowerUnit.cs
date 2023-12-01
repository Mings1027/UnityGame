using CustomEnumControl;
using EPOOutline;
using TowerControl;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UnitControl.TowerUnitControl
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

        private void Update()
        {
            if (Vector3.Distance(transform.position, navMeshAgent.destination) <=
                navMeshAgent.stoppingDistance)
            {
                _moveInput = false;
                navMeshAgent.stoppingDistance = 1;
                anim.SetBool(IsWalk, false);
                // anim.enabled = false;
                enabled = false;
            }
        }

        #endregion

        public void UnitTargetInit()
        {
            isAttacking = false;
            Move(_originPos);
            unitState = UnitState.Patrol;
            target = null;
            if (_moveInput) return;
            anim.SetBool(IsWalk, false);
        }

        public void Move(Vector3 pos)
        {
            enabled = true;
            // anim.enabled = true;
            _originPos = pos;
            _moveInput = true;
            anim.SetBool(IsWalk, true);
            navMeshAgent.stoppingDistance = 0.1f;
            navMeshAgent.SetDestination(pos);
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