using System;
using UnityEngine;

namespace ManagerControl
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
            _audioSource.PlayOneShot(_audioSource.clip);
        }
        
    }
}