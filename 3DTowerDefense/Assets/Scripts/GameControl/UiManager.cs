using System;
using PlayerControl;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GameControl
{
    public class UiManager : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private static bool onPointer;

        public static bool OnPointer => onPointer;


        public void OnPointerEnter(PointerEventData eventData)
        {
            onPointer = true;
            Debug.Log("ON!");
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            onPointer = false;
            Debug.Log("OFF!");
        }
    }
}