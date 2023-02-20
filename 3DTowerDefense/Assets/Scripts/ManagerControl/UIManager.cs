using UnityEngine;
using UnityEngine.EventSystems;

namespace ManagerControl
{
    public class UIManager : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public static bool Pointer { get; private set; }


        public void OnPointerEnter(PointerEventData eventData)
        {
            Pointer = true;
            print("On");
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            Pointer = false;
            print("Off");
        }
    }
}