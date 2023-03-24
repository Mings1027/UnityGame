using UnityEngine;

namespace ManagerControl
{
    public class SoundPlayer : MonoBehaviour
    {
        private AudioSource audioSource;
        // private AudioClip audioClip;

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
        }

        private void OnEnable()
        {
            var audioClip = audioSource.clip;
            audioSource.PlayOneShot(audioClip);
        }
    }
}