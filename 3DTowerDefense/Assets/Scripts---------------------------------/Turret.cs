using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Turret : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public event Action<Turret> onOpenEditPanelEvent;

    public void OnPointerDown(PointerEventData eventData)
    {
        
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        onOpenEditPanelEvent?.Invoke(this);
    }
}