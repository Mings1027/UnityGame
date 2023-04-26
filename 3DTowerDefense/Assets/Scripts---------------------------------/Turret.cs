using System;
using System.Collections;
using System.Collections.Generic;
using AttackControl;
using DG.Tweening;
using GameControl;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class Turret : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private TurretWeapon curTurret;

    private Tween atkDelayTween;
    private Collider[] targetColliders;
    private Transform target;
    private bool isTargeting;
    private bool attackAble;

    public bool IsUpgraded { get; set; }
    public Outline Outline { get; private set; }
    public event Action<Turret> onOpenEditPanelEvent;

    [SerializeField] private int smoothTurnSpeed;
    [SerializeField] private int minDamage, maxDamage;
    [SerializeField] private int atkRange;
    [SerializeField] private float atkDelay;
    [SerializeField] private LayerMask targetLayer;
    [SerializeField] private int rotateSpeed;

    private void Awake()
    {
        Outline = GetComponent<Outline>();
        curTurret = transform.GetChild(0).GetChild(0).GetComponent<TurretWeapon>();

        targetColliders = new Collider[5];
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
            var dir = target.position - curTurret.transform.position;
            var rot = Quaternion.LookRotation(new Vector3(dir.x, 0, dir.z));
            curTurret.transform.rotation =
                Quaternion.Slerp(curTurret.transform.rotation, rot, smoothTurnSpeed * Time.deltaTime);
            if (attackAble)
            {
                Attack();
                StartCoolDown();
            }
        }
        else
        {
            curTurret.transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime);
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

    private void TargetTracking()
    {
        var t = SearchTarget.ClosestTarget(transform.position, atkRange, targetColliders, targetLayer);
        target = t.Item1;
        isTargeting = t.Item2;
    }

    private void Attack()
    {
        curTurret.Attack();
    }

    private void StartCoolDown()
    {
        attackAble = false;
        atkDelayTween.Restart();
    }

    public void Upgrade()
    {
        atkDelayTween?.Kill();
        atkDelayTween = DOVirtual.DelayedCall(atkDelay, () => attackAble = true, false).SetAutoKill(false);
    }
}