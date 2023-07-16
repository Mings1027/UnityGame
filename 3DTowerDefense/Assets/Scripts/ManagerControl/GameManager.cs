using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ManagerControl
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private GameObject gamePlayPrefab;

        [SerializeField, Range(10, 150)] private int fontSize = 30;
        [SerializeField] private Color color;
        [SerializeField] private float width, height;

        private void Start()
        {
            Application.targetFrameRate = 60;
        }

        private void OnEnable()
        {
            Instantiate(gamePlayPrefab, transform);
        }

        private void OnGUI()
        {
            var position = new Rect(width, height, Screen.width, Screen.height);

            var fps = 1f / Time.deltaTime;
            var ms = Time.deltaTime * 1000f;
            var text = $"{fps:N1} FPS {ms:N1}ms";

            var style = new GUIStyle
            {
                fontSize = fontSize,
                normal =
                {
                    textColor = color
                }
            };

            GUI.Label(position, text, style);
        }
    }
}