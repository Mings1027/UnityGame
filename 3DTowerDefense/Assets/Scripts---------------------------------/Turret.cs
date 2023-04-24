using System;
using System.Collections;
using System.Collections.Generic;
using GameControl;
using UnityEngine;
using UnityEngine.EventSystems;

public class Turret : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private Transform[] turretBody;
    private Transform curTurret;

    [Range(0, 1)] public int type;
    public int BaseCount => turretBody.Length;

    [SerializeField] private int rotateSpeed;
    public event Action<Turret> onOpenEditPanelEvent;


    private void Awake()
    {
        turretBody = new Transform[transform.childCount];
        for (int i = 0; i < turretBody.Length; i++)
        {
            turretBody[i] = transform.GetChild(i);
        }

        if (turretBody.Length == 2)
        {
            turretBody[1].gameObject.SetActive(false);
        }
    }

    private void OnEnable()
    {
        curTurret = turretBody[0].GetChild(0);
        type = 0;
    }

    private void OnDisable()
    {
        StackObjectPool.ReturnToPool(gameObject);
    }

    private void Update()
    {
        curTurret.Rotate(Vector3.up, rotateSpeed * Time.deltaTime);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        onOpenEditPanelEvent?.Invoke(this);
    }

    public void ChangeTurretType(int index)
    {
        turretBody[index == 0 ? 1 : 0].gameObject.SetActive(false);
        turretBody[index].gameObject.SetActive(true);

        curTurret.gameObject.SetActive(false);
        curTurret = turretBody[index].GetChild(0);
        curTurret.gameObject.SetActive(true);

        type = index;
    }
}