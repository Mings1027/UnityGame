using System;
using GameControl;
using MapControl;
using UIControl;
using UnityEngine;

namespace ManagerControl
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private GameObject towerManagerObj;
        private void Awake()
        {
            Application.targetFrameRate = 60;
            Instantiate(towerManagerObj);
        }
    }
}