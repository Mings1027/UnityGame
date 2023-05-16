using System;
using DG.Tweening;
using GameControl;
using UnityEngine;
using UnityEngine.UI;

namespace ManagerControl
{
    public class GameManager : Singleton<GameManager>
    {
        private UIManager _uiManager;

        private void Awake()
        {
            _uiManager = UIManager.Instance;
        }
    }
}