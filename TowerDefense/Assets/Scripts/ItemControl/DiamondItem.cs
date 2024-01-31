using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace LobbyControl
{
    public class DiamondItem : MonoBehaviour
    {
        [field: SerializeField] public string productId { get; private set; }
    }
}