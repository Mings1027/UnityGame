using UnityEngine;

namespace SoundControl
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