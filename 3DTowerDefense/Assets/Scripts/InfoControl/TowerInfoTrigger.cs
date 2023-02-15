using System;
using ManagerControl;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

namespace InfoControl
{
    public class TowerInfoTrigger : MonoBehaviour, IPointerDownHandler
    {
        [SerializeField] private string header;
        [SerializeField] [Multiline] private string content;

        [SerializeField] private Transform infoPanelTransform;


        public void OnPointerDown(PointerEventData eventData)
        {
            TowerInfoSystem.Instance.Show(infoPanelTransform.position, content, header);
        }
    }
}