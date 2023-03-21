using System;
using UnityEngine;

namespace GameControl
{
    public class SoundPlayer : MonoBehaviour
    {
        private AudioSource _audioSource;

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
        }

        private void OnEnable()
        {
            _audioSource.Play();
        }
    }
}