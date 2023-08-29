using System;
using System.Collections.Generic;
using GameControl;
using UnityEngine;

namespace ManagerControl
{
    public class GameManager : Singleton<GameManager>
    {
        private void Awake()
        {
            Application.targetFrameRate = 60;
            var sources = Resources.LoadAll<GameObject>("Prefabs");
            for (var i = 0; i < sources.Length; i++)
            {
                Instantiate(sources[i]);
            }

        }
    }

    public enum TowerType
    {
        Ballista,
        Assassin,
        Canon,
        Mage,
        Defender
    }
}