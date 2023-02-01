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
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            OnPointer = false;
        }


    }
}