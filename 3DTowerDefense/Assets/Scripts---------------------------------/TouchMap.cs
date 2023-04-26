using System;
using System.Collections;
using System.Collections.Generic;
using ManagerControl;
using UnityEngine;
using UnityEngine.EventSystems;

public class TouchMap : MonoBehaviour, IPointerDownHandler,IPointerUpHandler
{
    public event Action onClosePanelEvent;
    
    public void OnPointerDown(PointerEventData eventData)
    {
        
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        onClosePanelEvent?.Invoke();
    }
}
