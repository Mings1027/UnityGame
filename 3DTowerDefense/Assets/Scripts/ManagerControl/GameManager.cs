using DG.Tweening;
using UnityEngine;

namespace ManagerControl
{
    public class GameManager : MonoBehaviour
    {
        private void Awake()
        {
            DOTween.SetTweensCapacity(500, 313);
        }
    }
}