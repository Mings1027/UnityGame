using System;
using UnityEngine;

namespace ManagerControl
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private GameObject gamePlayPrefab;

        private void Start()
        {
            // Application.targetFrameRate = 60;
        }

        private void OnEnable()
        {
            Instantiate(gamePlayPrefab, transform);
        }
    }
}