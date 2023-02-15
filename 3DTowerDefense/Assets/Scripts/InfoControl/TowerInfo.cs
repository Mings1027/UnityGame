using System;
using TMPro;
using UnityEngine;

namespace InfoControl
{
    public class TowerInfo : MonoBehaviour
    {
        private Camera _cam;
        [SerializeField] private TextMeshProUGUI headerField;
        [SerializeField] private TextMeshProUGUI contentField;

        private void Awake()
        {
            _cam = Camera.main;
            gameObject.SetActive(false);
        }

        private void Update()
        {
            var rotation = _cam.transform.rotation;
            transform.LookAt(transform.position + rotation * Vector3.forward,
                rotation * Vector3.up);
        }

        public void SetText(string content, string header = "")
        {
            if (string.IsNullOrEmpty(header))
            {
                headerField.gameObject.SetActive(false);
            }
            else
            {
                headerField.gameObject.SetActive(true);
                headerField.text = header;
            }

            contentField.text = content;
        }
    }
}