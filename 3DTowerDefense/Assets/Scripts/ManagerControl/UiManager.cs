using UnityEngine;
using UnityEngine.EventSystems;

namespace ManagerControl
{
    public class UiManager : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public static bool OnPointer { get; private set; }


        public void OnPointerEnter(PointerEventData eventData)
        {
            OnPointer = true;
            Debug.Log("ON!");
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            OnPointer = false;
            Debug.Log("OFF!");
        }


    }
}