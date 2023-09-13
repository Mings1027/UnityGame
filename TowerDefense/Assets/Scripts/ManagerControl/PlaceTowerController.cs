using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ManagerControl
{
    public class PlaceTowerController : MonoBehaviour, IDropHandler
    {
        public event Action OnPlaceTower;
        
        public void OnDrop(PointerEventData eventData)
        {
            OnPlaceTower?.Invoke();
            transform.position = Vector3.zero;
        }
    }
}