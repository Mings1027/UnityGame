using System;
using AttackControl;
using DG.Tweening;
using GameControl;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TurretControl
{
    public abstract class Turret : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        private Tween _atkDelayTween;
        private Collider[] _targetColliders;
        private bool _isTargeting;

        protected TurretWeapon Weapon;
        protected Transform Target;
        protected bool AttackAble;

        public bool IsUpgraded { get; set; }
        public Outline Outline { get; private set; }
        public event Action<Turret> onOpenEditPanelEvent;

        [SerializeField] private int minDamage, maxDamage;
        [SerializeField] private int atkRange;
        [SerializeField] private float atkDelay;
        [SerializeField] private LayerMask targetLayer;
        [SerializeField] private int rotateSpeed;


        protected virtual void Awake()
        {
            Outline = GetComponent<Outline>();
            Weapon = transform.GetChild(0).GetChild(0).GetComponent<TurretWeapon>();

            _targetColliders = new Collider[5];
            _atkDelayTween = DOVirtual.DelayedCall(atkDelay, () => AttackAble = true, false).SetAutoKill(false);
        }

        private void OnEnable()
        {
            Outline.enabled = false;

            InvokeRepeating(nameof(TargetTracking), 1f, 1f);
        }

        private void OnDisable()
        {
            StackObjectPool.ReturnToPool(gameObject);
            _atkDelayTween?.Kill();
            onOpenEditPanelEvent = null;
        }

        private void Update()
        {
            if (_isTargeting)
            {
                Targeting();
            }
            else
            {
                Weapon.transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime);
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            Outline.enabled = true;
            onOpenEditPanelEvent?.Invoke(this);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, atkRange);
        }

        protected abstract void Targeting();

        private void TargetTracking()
        {
            var t = SearchTarget.ClosestTarget(transform.position, atkRange, _targetColliders, targetLayer);
            Target = t.Item1;
            _isTargeting = t.Item2;
        }

        protected void Attack()
        {
            Weapon.Attack();
        }

        protected void StartCoolDown()
        {
            AttackAble = false;
            _atkDelayTween.Restart();
        }
    }
}