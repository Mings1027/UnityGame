using System;
using GameControl;
using MapControl;
using UIControl;
using UnityEngine;

namespace ManagerControl
{
    public class GameManager : MonoBehaviour
    {
        private void Awake()
        {
            Application.targetFrameRate = 60;
            var sources = Resources.LoadAll<GameObject>("Prefabs");
            for (int i = 0; i < sources.Length; i++)
            {
                Instantiate(sources[i]);
            }
        }
    }
}