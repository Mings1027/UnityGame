using System;
using UnityEngine;

namespace ManagerControl
{
    public class SoundPlayer : MonoBehaviour
    {
        private AudioSource audioSource;
        
        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
        }
        
        private void OnEnable()
        {
            audioSource.PlayOneShot(audioSource.clip);
        }
        
    }
}