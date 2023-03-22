using GameControl;
using UnityEngine;

namespace ManagerControl
{
    public class SoundController : Singleton<SoundController>
    {
        public SoundAudioClip[] soundAudioClipArray;

        [System.Serializable]
        public class SoundAudioClip
        {
            public SoundManager.Sound sound;
            public AudioClip audioClip;
        }
    }
}