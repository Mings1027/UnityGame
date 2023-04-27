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
        protected TurretWeapon weapon;
        private Tween atkDelayTween;
        private Collider[] targetColliders;
        protected Transform target;
        private bool isTargeting;
        protected bool attackAble;

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
            weapon = transform.GetChild(0).GetChild(0).GetComponent<TurretWeapon>();

            targetColliders = new Collider[5];
            atkDelayTween = DOVirtual.DelayedCall(atkDelay, () => attackAble = true, false).SetAutoKill(false);
        }

        private void OnEnable()
        {
            Outline.enabled = false;

            InvokeRepeating(nameof(TargetTracking), 1f, 1f);
        }

        private void OnDisable()
        {
            StackObjectPool.ReturnToPool(gameObject);
            atkDelayTween?.Kill();
            onOpenEditPanelEvent = null;
        }

        private void Update()
        {
            if (isTargeting)
            {
                Targeting();
            }
            else
            {
                weapon.transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime);
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
            var t = SearchTarget.ClosestTarget(transform.position, atkRange, targetColliders, targetLayer);
            target = t.Item1;
            isTargeting = t.Item2;
        }

        protected void Attack()
        {
            weapon.Attack();
        }

        protected void StartCoolDown()
        {
            attackAble = false;
            atkDelayTween.Restart();
        }
    }
}